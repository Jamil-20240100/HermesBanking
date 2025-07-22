using AutoMapper;
using HermesBanking.Core.Application.DTOs.Email;
using HermesBanking.Core.Application.DTOs.Loan;
using HermesBanking.Core.Application.DTOs.Transaction;
using HermesBanking.Core.Application.Interfaces;
using HermesBanking.Core.Application.ViewModels.Cashier;
using HermesBanking.Core.Application.ViewModels.SavingsAccount;
using HermesBanking.Core.Domain.Entities;
using HermesBanking.Core.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Threading.Tasks;


namespace HermesBanking.Core.Application.Services
{
    public class CashierService : ICashierService
    {
        private readonly ISavingsAccountRepository _accountRepo;
        private readonly ICashierTransactionService _cashierTransactionService;
        private readonly IMapper _mapper;
        private readonly IEmailService _emailService;
        private readonly IAccountServiceForWebApp _userService;
        private readonly ITransactionRepository _transactionRepo;
        private readonly ILoanRepository _loanRepo; // Asegúrate de que tienes un repositorio de préstamos
        private readonly ICreditCardRepository _cardRepo; // Asegúrate de que tienes un repositorio de tarjetas

        public CashierService(
            IAccountServiceForWebApp userService,
            ISavingsAccountRepository accountRepo,
            ICashierTransactionService cashierTransactionService,
            IMapper mapper,
            IEmailService emailService,
            ITransactionRepository transactionRepo,
            ILoanRepository loanRepo,
            ICreditCardRepository cardRepo)
        {
            _userService = userService;
            _accountRepo = accountRepo;
            _cashierTransactionService = cashierTransactionService;
            _mapper = mapper;
            _emailService = emailService;
            _transactionRepo = transactionRepo;
            _loanRepo = loanRepo; // Asignar repositorio de préstamos
            _cardRepo = cardRepo;
                    }

        private async Task SendTransactionEmailAsync(string accountNumber, decimal amount, string transactionType, string cashierId)
        {
            var account = await _accountRepo.GetByAccountNumberAsync(accountNumber);
            if (account == null) return;

            var user = await _userService.GetUserById(account.ClientId);
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
            var account = await _accountRepo.GetByAccountNumberAsync(accountNumber);
            if (account == null) return false;

            account.Balance += amount;
            await _accountRepo.UpdateAsync(account);

            var transactionDto = new TransactionDTO
            {
                SavingsAccountId = account.Id,
                Type = "DEPOSIT",
                Amount = amount,
                Origin = accountNumber,
                Beneficiary = accountNumber,
                CashierId = cashierId,
                Date = DateTime.Now,  // Fecha de la transacción
                TransactionDate = DateTime.Now // Aquí se asigna también el TransactionDate
            };

            await _cashierTransactionService.RegisterCashierTransactionAsync(transactionDto);

            await SendTransactionEmailAsync(accountNumber, amount, "Depósito", cashierId);
            return true;
        }

        public async Task<bool> MakeThirdPartyTransferAsync(string sourceAccountNumber, string destinationAccountNumber, decimal amount, string cashierId)
        {
            var sourceAccount = await _accountRepo.GetByAccountNumberAsync(sourceAccountNumber);
            var destinationAccount = await _accountRepo.GetByAccountNumberAsync(destinationAccountNumber);

            if (sourceAccount == null || destinationAccount == null || sourceAccount.Balance < amount)
                return false;

            sourceAccount.Balance -= amount;
            destinationAccount.Balance += amount;

            await _accountRepo.UpdateAsync(sourceAccount.Id, sourceAccount);
            await _accountRepo.UpdateAsync(destinationAccount.Id, destinationAccount);

            await _cashierTransactionService.ProcessCashierTransferAsync(sourceAccountNumber, destinationAccountNumber, amount, cashierId);

            await SendTransactionEmailAsync(sourceAccountNumber, amount, "Transferencia realizada", cashierId);
            await SendTransactionEmailAsync(destinationAccountNumber, amount, "Transferencia recibida", cashierId);

            return true;
        }

        public async Task<bool> MakeLoanPaymentAsync(string loanIdentifier, string accountNumber, decimal amount, string cashierId)
        {
            var loan = await _loanRepo.GetLoanByIdentifierAsync(loanIdentifier);
            if (loan == null || loan.RemainingDebt < amount) return false;

            loan.RemainingDebt -= amount;
            await _loanRepo.UpdateAsync(loan);

            var account = await _accountRepo.GetByAccountNumberAsync(accountNumber);
            if (account == null) return false;

            account.Balance -= amount;
            await _accountRepo.UpdateAsync(account);

            var transactionDto = new TransactionDTO
            {
                SavingsAccountId = account.Id,
                Type = "LOAN_PAYMENT",
                Amount = amount,
                Origin = accountNumber,
                Beneficiary = accountNumber,
                CashierId = cashierId,
                Date = DateTime.Now,  // Fecha de la transacción
                TransactionDate = DateTime.Now // Aquí se asigna también el TransactionDate
            };


            await _cashierTransactionService.RegisterCashierTransactionAsync(transactionDto);
            await SendTransactionEmailAsync(accountNumber, amount, "Pago de préstamo", cashierId);
            return true;
        }

        public async Task<List<SavingsAccount>> GetAllSavingsAccountsOfClients(string clientId)
        {
            return (List<SavingsAccount>)await _accountRepo.GetAccountsByClientIdAsync(clientId);
        }

        public async Task<CashierDashboardViewModel> GetTodaySummaryAsync(string cashierId)
        {
            var today = DateTime.Today;

            var transactions = await _transactionRepo
                .GetAllQuery()
                .Where(t => t.Date == today && t.CashierId == cashierId)
                .ToListAsync();

            var cashierTransactions = transactions.Where(t => t.CashierId == cashierId).ToList();

            var accounts = await _accountRepo.GetAllQuery()
                .Where(a => a.IsActive)
                .ToListAsync();

            var accountsViewModel = _mapper.Map<List<SavingsAccountViewModel>>(accounts);

            return new CashierDashboardViewModel
            {
                TotalTransactions = cashierTransactions.Count(),
                TotalDeposits = cashierTransactions.Count(t => t.Type == "DEPOSIT"),
                TotalWithdrawals = cashierTransactions.Count(t => t.Type == "WITHDRAWAL"),
                TotalPayments = cashierTransactions.Count(t => t.Type == "CREDIT_CARD_PAYMENT" || t.Type == "LOAN_PAYMENT"),
                Accounts = accountsViewModel
            };
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
                PendingAmount = loan.RemainingDebt
            }, clientFullName, loan.RemainingDebt);
        }


        public async Task<bool> MakeWithdrawAsync(string accountNumber, decimal amount, string cashierId)
        {
            var account = await _accountRepo.GetByAccountNumberAsync(accountNumber);
            if (account == null || account.Balance < amount) return false;

            account.Balance -= amount;
            await _accountRepo.UpdateAsync(account);

            var transactionDto = new TransactionDTO
            {
                SavingsAccountId = account.Id,
                Type = "WITHDRAWAL",
                Amount = amount,
                Origin = accountNumber,
                Beneficiary = accountNumber,
                CashierId = cashierId,
                Date = DateTime.Now,  // Fecha de la transacción
                TransactionDate = DateTime.Now // Aquí se asigna también el TransactionDate
            };

            await _cashierTransactionService.RegisterCashierTransactionAsync(transactionDto);
            await SendTransactionEmailAsync(accountNumber, amount, "Retiro", cashierId);
            return true;
        }

        public async Task<bool> MakeCreditCardPaymentAsync(string accountNumber, string cardNumber, decimal amount, string cashierId)
        {
            var account = await _accountRepo.GetByAccountNumberAsync(accountNumber);
            if (account == null || account.Balance < amount) return false;

            account.Balance -= amount;
            await _accountRepo.UpdateAsync(account);

            var transactionDto = new TransactionDTO
            {
                SavingsAccountId = account.Id,
                Type = "CREDIT_CARD_PAYMENT",
                Amount = amount,
                Origin = accountNumber,
                Beneficiary = accountNumber,
                CashierId = cashierId,
                Date = DateTime.Now,  // Fecha de la transacción
                TransactionDate = DateTime.Now // Aquí se asigna también el TransactionDate
            };


            await _cashierTransactionService.RegisterCashierTransactionAsync(transactionDto);
            
            await SendTransactionEmailAsync(accountNumber, amount, "Pago de tarjeta de crédito", cashierId);
            return true;
        }

        public async Task<SavingsAccount?> GetSavingsAccountByNumber(string accountNumber)
        {
            return await _accountRepo.GetByAccountNumberAsync(accountNumber);
        }

        public async Task<(SavingsAccount? account, string? clientFullName)> GetAccountWithClientNameAsync(string accountNumber)
        {
            var account = await _accountRepo.GetByAccountNumberAsync(accountNumber);
            var clientFullName = account != null ? $"{account.ClientFullName}" : null;

            return (account, clientFullName);
        }

        public async Task<(SavingsAccount? account, CreditCard? card, string? clientFullName)> GetAccountCardAndClientNameAsync(string accountNumber, string cardNumber)
        {
            var account = await _accountRepo.GetByAccountNumberAsync(accountNumber);
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
