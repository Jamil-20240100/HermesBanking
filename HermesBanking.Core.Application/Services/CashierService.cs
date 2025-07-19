using AutoMapper;
using HermesBanking.Core.Application.DTOs.Email;
using HermesBanking.Core.Application.DTOs.Loan;
using HermesBanking.Core.Application.DTOs.Transaction;
using HermesBanking.Core.Application.Interfaces;
using HermesBanking.Core.Application.ViewModels.Cashier;
using HermesBanking.Core.Domain.Entities;
using HermesBanking.Core.Domain.Interfaces;

namespace HermesBanking.Core.Application.Services
{
    public class CashierService : ICashierService
    {
        private readonly ISavingsAccountRepository _accountRepo;
        private readonly ITransactionService _transactionService;
        private readonly ICreditCardRepository _creditCardRepo;
        private readonly ILoanService _loanService;
        private readonly IMapper _mapper;
        private readonly IEmailService _emailService;
        private readonly IAccountServiceForWebApp _userService;

        public CashierService(
             IAccountServiceForWebApp userService,
            ISavingsAccountRepository accountRepo,
            ITransactionService transactionService,
            ICreditCardRepository creditCardRepo,
            ILoanService loanService,
            IMapper mapper,
            IEmailService emailService)
        {
            _userService = userService;
            _accountRepo = accountRepo;
            _transactionService = transactionService;
            _creditCardRepo = creditCardRepo;
            _loanService = loanService;
            _mapper = mapper;
            _emailService = emailService;
        }

        public async Task<bool> MakeDepositAsync(string accountNumber, decimal amount, string cashierId)
        {
            var account = await _accountRepo.GetByAccountNumberAsync(accountNumber);
            if (account == null) return false;

            account.Balance += amount;
            await _accountRepo.UpdateAsync(account);

            // Registrar transacción
            var transactionDto = new TransactionDTO
            {
                SavingsAccountId = account.Id,
                Type = "DEPOSIT",
                Amount = amount,
                Origin = accountNumber,
                Beneficiary = accountNumber,
                CashierId = cashierId,
                Date = DateTime.Now
            };

            await _transactionService.RegisterTransactionAsync(transactionDto);

            // Enviar correo de confirmación
            var user = await _userService.GetUserById(account.ClientId);
            if (user == null || string.IsNullOrEmpty(user.Email)) return false;

            string lastFour = account.AccountNumber.Substring(account.AccountNumber.Length - 4);
            string subject = $"Depósito realizado a su cuenta {lastFour}";
            string htmlBody = $@"
    <h3>Depósito exitoso</h3>
    <p>Se ha realizado un depósito a su cuenta <strong>{account.AccountNumber}</strong>.</p>
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

            return true;
        }

        public async Task<bool> MakeThirdPartyTransferAsync(string sourceAccountNumber, string destinationAccountNumber, decimal amount, string cashierId)
        {
            // Obtener las cuentas de origen y destino
            var sourceAccount = await _accountRepo.GetByAccountNumberAsync(sourceAccountNumber);
            var destinationAccount = await _accountRepo.GetByAccountNumberAsync(destinationAccountNumber);

            // Verificar que ambas cuentas existen y que la cuenta de origen tenga suficientes fondos
            if (sourceAccount == null || destinationAccount == null || sourceAccount.Balance < amount)
            {
                return false; // Retornar false si no se cumplen las condiciones
            }

            // Realizar la transferencia: deducir del saldo de la cuenta de origen y agregar al saldo de la cuenta de destino
            sourceAccount.Balance -= amount;
            destinationAccount.Balance += amount;

            // Actualizar las cuentas en el repositorio
            await _accountRepo.UpdateAsync(sourceAccount.Id, sourceAccount);
            await _accountRepo.UpdateAsync(destinationAccount.Id, destinationAccount);

            // Registrar la transacción de débito en la cuenta de origen
            await _transactionService.RegisterTransactionAsync(new TransactionDTO
            {
                SavingsAccountId = sourceAccount.Id,
                Type = "DÉBITO",
                Amount = amount,
                Origin = sourceAccountNumber,
                Beneficiary = destinationAccountNumber,
                CashierId = cashierId,
                Date = DateTime.Now
            });

            // Registrar la transacción de crédito en la cuenta de destino
            await _transactionService.RegisterTransactionAsync(new TransactionDTO
            {
                SavingsAccountId = destinationAccount.Id,
                Type = "CREDITO",
                Amount = amount,
                Origin = sourceAccountNumber,
                Beneficiary = destinationAccountNumber,
                CashierId = cashierId,
                Date = DateTime.Now
            });

            // Enviar notificación por correo electrónico
            var sourceUser = await _emailService.GetUserEmailByClientId(sourceAccount.ClientId);
            var destinationUser = await _emailService.GetUserEmailByClientId(destinationAccount.ClientId);

            if (sourceUser != null)
            {
                // Enviar correo al usuario de la cuenta de origen
                await _emailService.SendAsync(new EmailRequestDto
                {
                    To = sourceUser,
                    Subject = "Transferencia realizada",
                    HtmlBody = $"Se ha transferido RD${amount:N2} desde su cuenta {sourceAccountNumber} a la cuenta {destinationAccountNumber}."
                });
            }

            if (destinationUser != null)
            {
                // Enviar correo al usuario de la cuenta de destino
                await _emailService.SendAsync(new EmailRequestDto
                {
                    To = destinationUser,
                    Subject = "Transferencia recibida",
                    HtmlBody = $"Ha recibido RD${amount:N2} en su cuenta {destinationAccountNumber} desde la cuenta {sourceAccountNumber}."
                });
            }

            return true; // Retornar true si la transferencia fue exitosa
        }


        public async Task<bool> MakeWithdrawAsync(string accountNumber, decimal amount, string cashierId)
        {
            var account = await _accountRepo.GetByAccountNumberAsync(accountNumber);
            if (account == null || account.Balance < amount) return false;

            var user = await _userService.GetUserById(account.ClientId);
            if (user == null || string.IsNullOrEmpty(user.Email)) return false;

            account.Balance -= amount;
            await _accountRepo.UpdateAsync(account);

            await _transactionService.RegisterTransactionAsync(new TransactionDTO
            {
                SavingsAccountId = account.Id,
                Type = "WITHDRAWAL",
                Amount = amount,
                Origin = accountNumber,
                Beneficiary = accountNumber,
                CashierId = cashierId,
                Date = DateTime.Now
            });

            // Enviar correo de confirmación
            string lastFour = account.AccountNumber.Substring(account.AccountNumber.Length - 4);
            string subject = $"Retiro realizado de su cuenta {lastFour}";
            string htmlBody = $@"
    <h3>Retiro exitoso</h3>
    <p>Se ha realizado un retiro de su cuenta <strong>{account.AccountNumber}</strong>.</p>
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

            return true;
        }

        public async Task<bool> MakeCreditCardPaymentAsync(string accountNumber, string cardNumber, decimal amount, string cashierId)
        {
            var account = await _accountRepo.GetByAccountNumberAsync(accountNumber);
            var creditCard = await _creditCardRepo.GetByCardNumberAsync(cardNumber);

            if (account == null || creditCard == null || account.Balance < amount) return false;

            var paymentAmount = Math.Min(amount, creditCard.TotalOwedAmount);
            account.Balance -= paymentAmount;
            creditCard.TotalOwedAmount -= paymentAmount;

            await _accountRepo.UpdateAsync(account);
            await _creditCardRepo.UpdateAsync(creditCard);

            // Registrar transacción
            var transactionDto = new TransactionDTO
            {
                SavingsAccountId = account.Id,
                Type = "CREDIT_CARD_PAYMENT",
                Amount = paymentAmount,
                Origin = accountNumber,
                Beneficiary = cardNumber,
                CashierId = cashierId,
                Date = DateTime.Now
            };

            await _transactionService.RegisterTransactionAsync(transactionDto);
            return true;
        }

        // Implementación de otros métodos de la interfaz ICashierService
        public async Task<CashierDashboardViewModel> GetTodaySummaryAsync(string cashierId)
        {
            var today = DateTime.Today;
            var transactions = await _transactionService.GetTransactionsByCashierAndDateAsync(cashierId, today);

            return new CashierDashboardViewModel
            {
                TotalTransactions = transactions.Count,
                TotalDeposits = transactions.Count(t => t.Type == "DEPOSIT"),
                TotalWithdrawals = transactions.Count(t => t.Type == "WITHDRAWAL"),
                TotalPayments = transactions.Count(t => t.Type == "CREDIT_CARD_PAYMENT")
            };
        }

        // Métodos adicionales de la interfaz ICashierService

        public Task<bool> MakeLoanPaymentAsync(string loanIdentifier, string accountNumber, decimal amount, string cashierId)
        {
            // Implementar la lógica de pago de préstamo
            return Task.FromResult(true);
        }

        public Task<SavingsAccount?> GetSavingsAccountByNumber(string accountNumber)
        {
            return _accountRepo.GetByAccountNumberAsync(accountNumber);
        }

        public Task<(SavingsAccount? account, CreditCard? card, string? clientFullName)> GetAccountCardAndClientNameAsync(string accountNumber, string cardNumber)
        {
            // Implementar la lógica
            return Task.FromResult<(SavingsAccount? account, CreditCard? card, string? clientFullName)>((null, null, null));
        }

        public Task<(SavingsAccount? account, string? clientFullName)> GetAccountWithClientNameAsync(string accountNumber)
        {
            // Implementar la lógica
            return Task.FromResult<(SavingsAccount? account, string? clientFullName)>((null, null));
        }

        public Task<(LoanDTO? loan, string clientFullName, decimal remainingDebt)> GetLoanInfoAsync(string loanIdentifier)
        {
            // Implementar la lógica
            return Task.FromResult<(LoanDTO? loan, string clientFullName, decimal remainingDebt)>((null, "", 0m));
        }

        public Task<List<SavingsAccount>> GetAllActiveAccounts(string clientId)
        {
            // Implementar la lógica
            return Task.FromResult(new List<SavingsAccount>());
        }
    }
}
