using AutoMapper;
using HermesBanking.Core.Application.DTOs.CreditCard;
using HermesBanking.Core.Application.DTOs.Email;
using HermesBanking.Core.Application.DTOs.Loan;
using HermesBanking.Core.Application.DTOs.Transaction;
using HermesBanking.Core.Application.Interfaces;
using HermesBanking.Core.Application.ViewModels.Cashier;
using HermesBanking.Core.Domain.Common.Enums;
using HermesBanking.Core.Domain.Entities;
using HermesBanking.Core.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;


namespace HermesBanking.Core.Application.Services
{

    public class CashierService : ICashierService
    {
        private readonly ILoanService _loanService;
        private readonly IMapper _mapper;
        private readonly ISavingsAccountRepository _accountRepo;
        private readonly ITransactionService _transactionService;
        private readonly IAccountServiceForWebApp _userService;
        private readonly IEmailService _emailService;
        private readonly ICreditCardRepository _creditCardRepo;
        private readonly ILoanRepository _loanRepo;
        private readonly IAmortizationInstallmentRepository _installmentRepo;
        private readonly ILogger<CashierService> _logger;


        public CashierService(
            ISavingsAccountRepository accountRepo,
            ILoanService loanService,
            IMapper mapper,
            ITransactionService transactionService,
            IAccountServiceForWebApp userService,
            IEmailService emailService,
            ICreditCardRepository creditCardRepo,
            ILoanRepository loanRepo,
            IAmortizationInstallmentRepository installmentRepo,
            ILogger<CashierService> logger)
        {
            _accountRepo = accountRepo;
            _loanService = loanService;
            _mapper = mapper;
            _transactionService = transactionService;
            _userService = userService;
            _emailService = emailService;
            _creditCardRepo = creditCardRepo;
            _loanRepo = loanRepo;
            _installmentRepo = installmentRepo;
            _logger = logger;
        }
        

        public async Task<bool> MakeDepositAsync(string accountNumber, decimal amount, string cashierId)
        {
            var account = _accountRepo
                .GetAllQuery()
                .FirstOrDefault(a => a.AccountNumber == accountNumber && a.IsActive);

            if (account == null) return false;

            var user = await _userService.GetUserById(account.ClientId);
            if (user == null || string.IsNullOrEmpty(user.Email)) return false;

            account.Balance += amount;
            await _accountRepo.UpdateAsync(account.Id, account);

            await _transactionService.RegisterTransactionAsync(
                savingsAccountId: account.Id,
                type: "CRÉDITO",
                amount: amount,
                origin: "DEPÓSITO",
                beneficiary: account.AccountNumber,
                cashierId: cashierId
            );

            // Formatear los datos del correo
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


        public SavingsAccount? GetSavingsAccountByNumber(string accountNumber)
        {
            return _accountRepo
                .GetAllQuery()
                .FirstOrDefault(a => a.AccountNumber == accountNumber && a.IsActive);
        }

        public async Task<(SavingsAccount? account, CreditCard? card, string? clientFullName)> GetAccountCardAndClientNameAsync(string accountNumber, string cardNumber)
        {
            var account = _accountRepo.GetAllQuery()
                .FirstOrDefault(a => a.AccountNumber == accountNumber && a.IsActive);

            var card = _creditCardRepo.GetAllQuery()
                .FirstOrDefault(c => c.CardId == cardNumber && c.IsActive);

            if (account == null || card == null)
                return (null, null, null);

            var user = await _userService.GetUserById(card.ClientId);
            string fullName = user != null ? $"{user.Name} {user.LastName}" : null;

            return (account, card, fullName);
        }

        public async Task<bool> MakeWithdrawAsync(string accountNumber, decimal amount, string cashierId)
        {
            var account = _accountRepo
                .GetAllQuery()
                .FirstOrDefault(a => a.AccountNumber == accountNumber && a.IsActive);

            if (account == null || account.Balance < amount) return false;

            var user = await _userService.GetUserById(account.ClientId);
            if (user == null || string.IsNullOrEmpty(user.Email)) return false;

            account.Balance -= amount;
            await _accountRepo.UpdateAsync(account.Id, account);

            await _transactionService.RegisterTransactionAsync(
                savingsAccountId: account.Id,
                type: "DÉBITO",
                amount: amount,
                origin: account.AccountNumber,
                beneficiary: "RETIRO",
                cashierId: cashierId
            );

            // Correo
            string lastFour = account.AccountNumber[^4..];
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

        public async Task<bool> MakeThirdPartyTransferAsync(string sourceAccountNumber, string destinationAccountNumber, decimal amount, string cashierId)
        {
            var sourceAccount = await _accountRepo
                .GetAllQuery()
                .Where(a => a.AccountNumber == sourceAccountNumber && a.IsActive)
                .AsTracking()
                .FirstOrDefaultAsync();

            var destAccount = await _accountRepo
                .GetAllQuery()
                .Where(a => a.AccountNumber == destinationAccountNumber && a.IsActive)
                .AsTracking()
                .FirstOrDefaultAsync();

            if (sourceAccount == null || destAccount == null) return false;
            if (sourceAccount.Balance < amount) return false;

            var sourceUser = await _userService.GetUserById(sourceAccount.ClientId);
            var destUser = await _userService.GetUserById(destAccount.ClientId);

            if (sourceUser == null || destUser == null) return false;

            // Actualizar balances
            sourceAccount.Balance -= amount;
            destAccount.Balance += amount;

            await _accountRepo.UpdateAsync(sourceAccount.Id, sourceAccount);
            await _accountRepo.UpdateAsync(destAccount.Id, destAccount);

            await _transactionService.RegisterTransactionAsync(new Transaction
            {
                SavingsAccountId = sourceAccount.Id,
                Type = "DÉBITO",
                Amount = amount,
                Origin = sourceAccount.AccountNumber,
                Beneficiary = destAccount.AccountNumber,
                PerformedByCashierId = cashierId,
                Date = DateTime.Now
            });


            await _transactionService.RegisterTransactionAsync(
                destAccount.Id,
                "CRÉDITO",
                amount,
                sourceAccount.AccountNumber,
                destAccount.AccountNumber,
                cashierId
            );



            // Correos electrónicos
            string last4Dest = destAccount.AccountNumber[^4..];
            string last4Src = sourceAccount.AccountNumber[^4..];

            await _emailService.SendAsync(new EmailRequestDto
            {
                To = sourceUser.Email,
                Subject = $"Transacción realizada a la cuenta {last4Dest}",
                HtmlBody = $@"
            <h3>Transferencia enviada</h3>
            <p>Se ha transferido RD$ {amount:N2} a la cuenta {destAccount.AccountNumber}.</p>
            <p><strong>Fecha:</strong> {DateTime.Now:dd/MM/yyyy} <strong>Hora:</strong> {DateTime.Now:hh:mm tt}</p>"
            });

            await _emailService.SendAsync(new EmailRequestDto
            {
                To = destUser.Email,
                Subject = $"Transacción enviada desde la cuenta {last4Src}",
                HtmlBody = $@"
            <h3>Transferencia recibida</h3>
            <p>Has recibido RD$ {amount:N2} desde la cuenta {sourceAccount.AccountNumber}.</p>
            <p><strong>Fecha:</strong> {DateTime.Now:dd/MM/yyyy} <strong>Hora:</strong> {DateTime.Now:hh:mm tt}</p>"
            });

            return true;
        }

        public async Task<CashierDashboardViewModel> GetTodaySummaryAsync(string cashierId)
        {
            var today = DateTime.Today;

            var allAccounts = GetAllActiveAccounts();

            var allToday = await _transactionService.GetTransactionsByCashierAndDateAsync(cashierId, today);

            var dashboard = new CashierDashboardViewModel
            {
                TotalTransactions = allToday.Count,
                TotalDeposits = allToday.Count(t => t.Origin == "DEPÓSITO"),
                TotalWithdrawals = allToday.Count(t => t.Beneficiary == "RETIRO"),

                // CORRECCIÓN: Pago debe contar bien según el campo Type, Origin o Beneficiary, aquí te pongo Type
                TotalPayments = allToday.Count(t =>
                    !string.IsNullOrEmpty(t.Type) &&
                    (t.Type.StartsWith("TC") || t.Type.StartsWith("PRÉSTAMO") || t.Type.StartsWith("PAGO"))),

                Accounts = allAccounts
            };

            return dashboard;
        }



        public async Task<bool> MakeCreditCardPaymentAsync(string accountNumber, string cardNumber, decimal amount, string cashierId)
        {
            // Obtener cuenta activada
            var cuenta = await _accountRepo.GetAllQuery()
                .FirstOrDefaultAsync(a => a.AccountNumber == accountNumber && a.IsActive);

            // Obtener tarjeta activada
            var tarjeta = await _creditCardRepo.GetAllQuery()
                .FirstOrDefaultAsync(t => t.CardId == cardNumber && t.IsActive);

            // Validar existencia de cuenta y tarjeta y que el balance sea suficiente
            if (cuenta == null || tarjeta == null || cuenta.Balance < amount)
                return false;

            // Verificar la deuda pendiente y pagar hasta el monto disponible
            decimal pagoReal = Math.Min(amount, tarjeta.TotalOwedAmount);  // Paga hasta la deuda pendiente

            // Debitar la cuenta del cajero
            cuenta.Balance -= pagoReal;
            await _accountRepo.UpdateAsync(cuenta.Id, cuenta);

            // Reducir la deuda de la tarjeta
            tarjeta.TotalOwedAmount -= pagoReal;
            await _creditCardRepo.UpdateAsync(tarjeta.Id, tarjeta);

            // Registrar transacción de débito en la cuenta
            await _transactionService.RegisterTransactionAsync(new Transaction
            {
                SavingsAccountId = cuenta.Id,
                Type = "PAGO TARJETA DE CRÉDITO", // Tipo de transacción
                Amount = pagoReal,
                Origin = cuenta.AccountNumber,
                Beneficiary = tarjeta.CardId,
                PerformedByCashierId = cashierId,
                Date = DateTime.Now
            });

            // Enviar correo al cliente
            string last4Tarjeta = tarjeta.CardId[^4..];  // Últimos 4 dígitos de la tarjeta
            string last4Cuenta = cuenta.AccountNumber[^4..]; // Últimos 4 dígitos de la cuenta
            string subject = $"Pago realizado a la tarjeta {last4Tarjeta}";

            string html = $@"
<h3>Pago exitoso</h3>
<p>Has pagado RD$ {pagoReal:N2} desde tu cuenta {cuenta.AccountNumber} a tu tarjeta {tarjeta.CardId}.</p>
<ul>
    <li><strong>Fecha:</strong> {DateTime.Now:dd/MM/yyyy}</li>
    <li><strong>Hora:</strong> {DateTime.Now:hh:mm tt}</li>
</ul>";

            var email = new EmailRequestDto
            {
                To = await _userService.GetUserEmailAsync(tarjeta.ClientId), // Asegúrate de que el servicio devuelva el correo del cliente
                Subject = subject,
                HtmlBody = html
            };

            await _emailService.SendAsync(email);

            return true;
        }



        public async Task<string> GetUserEmailByClientIdAsync(string clientId)
        {
            var user = await _userService.GetUserById(clientId);
            return user?.Email ?? string.Empty;
        }


        public async Task<(SavingsAccount? account, string? clientFullName)> GetAccountWithClientNameAsync(string accountNumber)
        {
            var account = _accountRepo
                .GetAllQuery()
                .FirstOrDefault(a => a.AccountNumber == accountNumber && a.IsActive);

            if (account == null) return (null, null);

            var user = await _userService.GetUserById(account.ClientId);

            if (user == null) return (account, null);

            string fullName = $"{user.Name} {user.LastName}";

            return (account, fullName);
        }

        public async Task<bool> MakeLoanPaymentAsync(string loanIdentifier, string accountNumber, decimal amount, string cashierId)
        {
            var account = _accountRepo
                .GetAllQuery()
                .FirstOrDefault(a => a.AccountNumber == accountNumber && a.IsActive);

            if (account == null || account.Balance < amount) return false;

            var loans = await _loanService.GetAllLoansAsync(null, "active");
            var loan = loans.FirstOrDefault(l => l.LoanIdentifier == loanIdentifier);

            if (loan == null || !loan.IsActive) return false;

            var amortizationSchedule = await _loanService.GetAmortizationTableByLoanIdAsync(loan.Id);
            var nextInstallment = amortizationSchedule
                .Where(i => !i.IsPaid)
                .OrderBy(i => i.InstallmentNumber)
                .FirstOrDefault();

            if (nextInstallment == null || amount < nextInstallment.InstallmentValue)
                return false;

            // Descontar dinero de la cuenta
            account.Balance -= nextInstallment.InstallmentValue;
            await _accountRepo.UpdateAsync(account.Id, account);

            // Registrar transacción
            await _transactionService.RegisterTransactionAsync(new()
            {
                Amount = nextInstallment.InstallmentValue,
                Date = DateTime.Now,
                Description = $"Pago cuota préstamo #{loan.LoanIdentifier}",
                CashierId = cashierId,
                ClientId = loan.ClientId,
                TransactionType = Domain.Common.Enums.TransactionType.LoanPayment
            });

            nextInstallment.IsPaid = true;
            nextInstallment.PaidDate = DateTime.Now;
            await _installmentRepo.UpdateAsync(_mapper.Map<AmortizationInstallment>(nextInstallment));

            if (loan.PaidInstallments + 1 >= loan.TotalInstallments)
            {
                var loanEntity = _mapper.Map<Loan>(loan);
                loanEntity.IsActive = false;
                loanEntity.CompletedAt = DateTime.Now;
                await _loanRepo.UpdateAsync(loanEntity);
            }

            return true;
        }



        public async Task<(LoanDTO? loan, string clientFullName, decimal remainingDebt)> GetLoanInfoAsync(string loanIdentifier)
        {
            var loans = await _loanService.GetAllLoansAsync(null, "active");
            var loan = loans.FirstOrDefault(l => l.LoanIdentifier == loanIdentifier);

            if (loan == null)
                return (null, string.Empty, 0m);

            return (loan, loan.ClientFullName, loan.PendingAmount);
        }


        public List<SavingsAccount> GetAllActiveAccounts()
        {
            return _accountRepo.GetAllQuery().Where(a => a.IsActive).ToList();
        }

        
    }
}
