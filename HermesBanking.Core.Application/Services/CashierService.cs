using AutoMapper;
using HermesBanking.Core.Application.DTOs.Cashier;
using HermesBanking.Core.Application.DTOs.Email;
using HermesBanking.Core.Application.DTOs.Loan;
using HermesBanking.Core.Application.DTOs.Transaction;
using HermesBanking.Core.Application.Interfaces;
using HermesBanking.Core.Application.Mappings.DTOsAndViewModels;
using HermesBanking.Core.Application.ViewModels.Cashier;
using HermesBanking.Core.Application.ViewModels.SavingsAccount;
using HermesBanking.Core.Domain.Common.Enums;
using HermesBanking.Core.Domain.Entities;

using HermesBanking.Core.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Threading.Tasks;




namespace HermesBanking.Core.Application.Services
{
    public class CashierService : ICashierService
    {
        private readonly ISavingsAccountRepository _savingsAccountRepository;
        private readonly ICashierTransactionService _cashierTransactionService;
        private readonly IMapper _mapper;
        private readonly IEmailService _emailService;
        private readonly IAccountServiceForWebApp _accountServiceForWebApp;
        private readonly ITransactionRepository _transactionRepo;
        private readonly ILoanRepository _loanRepo; // Asegúrate de que tienes un repositorio de préstamos
        private readonly ICreditCardRepository _cardRepo; // Asegúrate de que tienes un repositorio de tarjetas
        private readonly ILogger<TransactionService> _logger;
        private readonly ILoanAmortizationService _loanAmortizationService;

        public CashierService(
            IAccountServiceForWebApp userService,
            ISavingsAccountRepository accountRepo,
            ICashierTransactionService cashierTransactionService,
            IMapper mapper,
            IEmailService emailService,
            ITransactionRepository transactionRepo,
            ILoanRepository loanRepo,
            ICreditCardRepository cardRepo,
            ILoanAmortizationService loanAmortizationService,
            ILogger<TransactionService> logger)
        {
            _accountServiceForWebApp = userService;
            _savingsAccountRepository = accountRepo;
            _cashierTransactionService = cashierTransactionService;
            _mapper = mapper;
            _emailService = emailService;
            _transactionRepo = transactionRepo;
            _loanRepo = loanRepo; // Asignar repositorio de préstamos
            _cardRepo = cardRepo;
            _logger = logger;
            _loanAmortizationService = loanAmortizationService;
        }

        private async Task SendTransactionEmailAsync(string accountNumber, decimal amount, string transactionType, string cashierId)
        {
            var account = await _savingsAccountRepository.GetByAccountNumberAsync(accountNumber);
            if (account == null)    return;

            var user = await _accountServiceForWebApp.GetUserById(account.ClientId);
            if (user == null || string.IsNullOrEmpty(user.Email)) return;

            string lastFour = account.AccountNumber.Substring(account.AccountNumber.Length - 4);
            string subject = $"{transactionType} realizado a su cuenta {lastFour}";
            string htmlBody = $@"
                <h3>{transactionType} exitoso</h3>
                <p>Se ha realizado una transacción de tipo <strong>{transactionType}</strong> a su cuenta <strong>{account.AccountNumber}</strong>.</p>
                <ul>
                    <li><strong>Monto:</strong> RD$ {amount:N2}</li>
                    <li><strong>Fecha:</strong> {DateTime.Now:dd/MM/yyyy}</li>
                    <li><strong>Hora:</strong> {DateTime.Now:hh:mm tt}</li>
                </ul>";

            var email = new EmailRequestDto
            {
                To = user.Email,
                Subject = subject,
                HtmlBody = htmlBody
            };

            await _emailService.SendAsync(email);
        }


        public async Task<bool> MakeDepositAsync(string accountNumber, decimal amount, string cashierId)
        {
            var account = await _savingsAccountRepository.GetByAccountNumberAsync(accountNumber);
            if (account == null) return false;

            account.Balance += amount;
            await _savingsAccountRepository.UpdateAsync(account);

            var transactionDto = new TransactionDTO
            {
                SavingsAccountId = account.Id,
                Type = TransactionType.CREDITO.ToString(),
                TransactionType = TransactionType.Deposito,
                Amount = amount,
                Origin = accountNumber,
                Beneficiary = accountNumber,
                Status = Status.APPROVED,
                CashierId = cashierId,
                Date = DateTime.Now,  // Fecha de la transacción
                TransactionDate = DateTime.Now // Aquí se asigna también el TransactionDate
            };
            
            await _cashierTransactionService.RegisterCashierTransactionAsync(transactionDto);

            await SendTransactionEmailAsync(accountNumber, amount, TransactionType.Deposito.ToString(), cashierId);
            return true;
        }

        public async Task<bool> MakeThirdPartyTransferAsync(string sourceAccountNumber, string destinationAccountNumber, decimal amount, string cashierId)
        {
            var sourceAccount = await _savingsAccountRepository.GetByAccountNumberAsync(sourceAccountNumber);
            var destinationAccount = await _savingsAccountRepository.GetByAccountNumberAsync(destinationAccountNumber);

            if (sourceAccount == null || destinationAccount == null || sourceAccount.Balance < amount)
                return false;

            sourceAccount.Balance -= amount;
            destinationAccount.Balance += amount;

            await _savingsAccountRepository.UpdateAsync(sourceAccount.Id, sourceAccount);
            await _savingsAccountRepository.UpdateAsync(destinationAccount.Id, destinationAccount);

            await _cashierTransactionService.ProcessCashierTransferAsync(sourceAccountNumber, destinationAccountNumber, amount, cashierId);

            await SendTransactionEmailAsync(sourceAccountNumber, amount, "Transferencia realizada", cashierId);
            await SendTransactionEmailAsync(destinationAccountNumber, amount, "Transferencia recibida", cashierId);

            return true;
        }

        public async Task<bool> MakeLoanPaymentAsync(LoanPaymentByCashierDto paymentDto)
         {
            _logger.LogInformation($"Iniciando pago de préstamo: Préstamo={paymentDto.LoanIdentifier}, Origen={paymentDto.SourceAccountNumber}, Monto={paymentDto.Amount}");

            var loan = await _accountServiceForWebApp.GetLoanInfoAsync(paymentDto.LoanIdentifier);

            if (loan == null) throw new InvalidOperationException("Préstamo no encontrado.");

        
            var sourceAccount = await _savingsAccountRepository.GetByAccountNumberAsync(paymentDto.SourceAccountNumber);
            if (sourceAccount == null) throw new InvalidOperationException("Cuenta de origen no encontrada.");


            if (!sourceAccount.IsActive) throw new InvalidOperationException("La cuenta de origen está inactiva.");
            // Ajusta según cómo manejes el estado de un préstamo completado/inactivo
            if (!loan.IsActive) throw new InvalidOperationException("El préstamo ya no está activo o está completado.");


            // 2. Verificar fondos suficientes
            if (sourceAccount.Balance < paymentDto.Amount)
            {
                throw new InvalidOperationException("Fondos insuficientes en la cuenta de origen.");
            }

            sourceAccount.Balance -= paymentDto.Amount;
            await _savingsAccountRepository.UpdateAsync(sourceAccount);

            decimal amountAppliedToLoan = await _loanAmortizationService.ApplyPaymentToLoanAsync(loan.Id, paymentDto.Amount);

            var transactionDto = new TransactionDTO
            {
                SavingsAccountId = sourceAccount.Id,
                Type = TransactionType.DEBITO.ToString(),
                TransactionType = TransactionType.PagoPrestamo,
                Beneficiary = sourceAccount.AccountNumber,
                Amount = paymentDto.Amount,
                Origin = $"LoanPayment CDA ...{sourceAccount?.AccountNumber?.Substring(sourceAccount.AccountNumber.Length - 4)}",
                Description = $"Pago de préstamo {paymentDto.LoanIdentifier}. Realizado por el cajero {paymentDto.CashierId:C}",
                Status = Status.APPROVED,
                CashierId = paymentDto.CashierId,
                DestinationLoanId = loan.Id,
                Date = DateTime.Now,  // Fecha de la transacción
                TransactionDate = DateTime.Now // Aquí se asigna también el TransactionDate
            };


            await _cashierTransactionService.RegisterCashierTransactionAsync(transactionDto);
            // 6. Enviar notificación por correo
            var clientEmail = await _accountServiceForWebApp.GetUserEmailAsync(sourceAccount.ClientId);
            if (!string.IsNullOrEmpty(clientEmail))
            {
                await _emailService.SendLoanPaymentNotificationAsync(
                    clientEmail,
                    paymentDto.Amount,
                    paymentDto.LoanIdentifier,
                    paymentDto.SourceAccountNumber.Substring(paymentDto.SourceAccountNumber.Length - 4),
                    transactionDto.TransactionDate 
                );
            }
            //await SendTransactionEmailAsync(transactionDto.Beneficiary.ToString(), transactionDto.Amount, TransactionType.PagoPrestamo.ToString(), transactionDto.CashierId);
            return true;
        }

        public async Task<List<SavingsAccount>> GetAllSavingsAccountsOfClients(string clientId)
        {
            return (List<SavingsAccount>)await _savingsAccountRepository.GetAccountsByClientIdAsync(clientId);
        }

        public async Task<CashierDashboardViewModel> GetTodaySummaryAsync(string cashierId)
        {
            var today = DateTime.Today;

            var transactions = await _transactionRepo
                .GetByCashierId(cashierId, today);

            var cashierTransactions = transactions.Where(t => t.CashierId == cashierId).ToList();

            var accounts = await _savingsAccountRepository.GetAllQuery()
                .Where(a => a.IsActive)
                .ToListAsync();

            var accountsViewModel = _mapper.Map<List<SavingsAccountViewModel>>(accounts);
            var whatev = new CashierDashboardViewModel { };


            whatev.TotalTransactions = cashierTransactions.Count(t => t.TransactionType == TransactionType.Deposito || t.TransactionType == TransactionType.Retiro || t.TransactionType == TransactionType.PagoTarjetaCredito || t.TransactionType == TransactionType.PagoPrestamo);
            whatev.TotalDeposits = cashierTransactions.Count(t => t.TransactionType == TransactionType.Deposito);
            whatev.TotalWithdrawals = cashierTransactions.Count(t => t.TransactionType == TransactionType.Retiro);
            whatev.TotalPayments = cashierTransactions.Count(t => t.TransactionType == TransactionType.PagoTarjetaCredito || t.TransactionType == TransactionType.PagoPrestamo);
            whatev.Accounts = accountsViewModel;


            return whatev;
        }

        public async Task<(LoanDTO? loan, string clientFullName, decimal remainingDebt)> GetLoanInfoAsync(string loanIdentifier)
        {
            var loan = await _loanRepo.GetLoanByIdentifierAsync(loanIdentifier);
            if (loan == null) return (null, null, 0);

            var clientFullName = $"{loan.ClientFullName}"; // Aquí puedes ajustar el formato del nombre si es necesario
            return (new LoanDTO
            {
                LoanIdentifier = loan.LoanIdentifier,
                Amount = loan.Amount,
                PendingAmount = loan.RemainingDebt,
                IsActive = loan.IsActive,

            }, clientFullName, loan.RemainingDebt);
        }


        public async Task<bool> MakeWithdrawAsync(string accountNumber, decimal amount, string cashierId)
        {
            var account = await _savingsAccountRepository.GetByAccountNumberAsync(accountNumber);
            if (account == null || account.Balance < amount) return false;

            account.Balance -= amount;
            await _savingsAccountRepository.UpdateAsync(account);

            var transactionDto = new TransactionDTO
            {
                SavingsAccountId = account.Id,
                Type = TransactionType.DEBITO.ToString(),
                TransactionType = TransactionType.Retiro,
                Amount = amount,
                Origin = accountNumber,
                Beneficiary = accountNumber,
                CashierId = cashierId,
                Status = Status.APPROVED,
                Date = DateTime.Now,  // Fecha de la transacción
                TransactionDate = DateTime.Now // Aquí se asigna también el TransactionDate
            };

            await _cashierTransactionService.RegisterCashierTransactionAsync(transactionDto);
            await SendTransactionEmailAsync(accountNumber, amount, "Retiro", cashierId);
            return true;
        }

        public async Task<bool> MakeCreditCardPaymentAsync(string accountNumber, string cardNumber, decimal amount, string cashierId)
        {
            var account = await _savingsAccountRepository.GetByAccountNumberAsync(accountNumber);
            if (account == null || account.Balance < amount) return false;

            account.Balance -= amount;
            await _savingsAccountRepository.UpdateAsync(account);

            var transactionDto = new TransactionDTO
            {
                SavingsAccountId = account.Id,
                Type = TransactionType.DEBITO.ToString(),
                TransactionType = TransactionType.PagoTarjetaCredito,
                Amount = amount,
                Origin = accountNumber,
                Beneficiary = accountNumber,
                CashierId = cashierId,
                Status = Status.APPROVED,
                Date = DateTime.Now,  // Fecha de la transacción
                TransactionDate = DateTime.Now // Aquí se asigna también el TransactionDate
            };


            await _cashierTransactionService.RegisterCashierTransactionAsync(transactionDto);
            
            await SendTransactionEmailAsync(accountNumber, amount, "Pago de tarjeta de crédito", cashierId);
            return true;
        }

        public async Task<SavingsAccount?> GetSavingsAccountByNumber(string accountNumber)
        {
            return await _savingsAccountRepository.GetByAccountNumberAsync(accountNumber);
        }

        public async Task<(SavingsAccount? account, string? clientFullName)> GetAccountWithClientNameAsync(string accountNumber)
        {
            var account = await _savingsAccountRepository.GetByAccountNumberAsync(accountNumber);
            var clientFullName = account != null ? $"{account.ClientFullName}" : null;

            return (account, clientFullName);
        }

        public async Task<(SavingsAccount? account, CreditCard? card, string? clientFullName)> GetAccountCardAndClientNameAsync(string accountNumber, string cardNumber)
        {
            var account = await _savingsAccountRepository.GetByAccountNumberAsync(accountNumber);
            var card = await _cardRepo.GetByCardNumberAsync(cardNumber);

            // Si la cuenta o tarjeta no existen, retornamos null
            if (account == null || card == null)
            {
                return (null, null, null);
            }

            // Se devuelve el nombre completo del cliente, puedes ajustar esto según tu modelo
            var clientFullName = account.ClientFullName;

            return (account, card, clientFullName);
        }

    }
}
