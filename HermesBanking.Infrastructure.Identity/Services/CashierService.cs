using HermesBanking.Core.Application.DTOs.Email;
using HermesBanking.Core.Application.Interfaces;
using HermesBanking.Core.Application.ViewModels.Cashier;
using HermesBanking.Core.Domain.Entities;
using HermesBanking.Core.Domain.Interfaces;
using HermesBanking.Infrastructure.Identity.Entities;
using HermesBanking.Infrastructure.Identity.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace HermesBanking.Infrastructure.Persistence.Services
{
    public class CashierService : ICashierService
    {
        private readonly ISavingsAccountRepository _accountRepo;
        private readonly ITransactionService _transactionService;
        private readonly UserManager<AppUser> _userManager;
        private readonly IEmailService _emailService;
        private readonly ICreditCardRepository _creditCardRepo;
        private readonly ILoanRepository _loanRepo;
        private readonly ILoanInstallmentRepository _installmentRepo;


        public CashierService(
            ISavingsAccountRepository accountRepo,
            ITransactionService transactionService,
            UserManager<AppUser> userManager,
            IEmailService emailService,
            ICreditCardRepository creditCardRepo,
            ILoanRepository loanRepo,
            ILoanInstallmentRepository installmentRepo)
        {
            _accountRepo = accountRepo;
            _transactionService = transactionService;
            _userManager = userManager;
            _emailService = emailService;
            _creditCardRepo = creditCardRepo;
            _loanRepo = loanRepo;
            _installmentRepo = installmentRepo;
        }


        public async Task<bool> MakeDepositAsync(string accountNumber, decimal amount, string cashierId)
        {
            var account = _accountRepo
                .GetAllQuery()
                .FirstOrDefault(a => a.AccountNumber == accountNumber && a.IsActive);

            if (account == null) return false;

            var user = await _userManager.FindByIdAsync(account.ClientId);
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
                .FirstOrDefault(c => c.CardNumber == cardNumber && c.IsActive);

            if (account == null || card == null)
                return (null, null, null);

            var user = await _userManager.FindByIdAsync(card.ClientId);
            string fullName = user != null ? $"{user.Name} {user.LastName}" : null;

            return (account, card, fullName);
        }

        public async Task<bool> MakeWithdrawAsync(string accountNumber, decimal amount, string cashierId)
        {
            var account = _accountRepo
                .GetAllQuery()
                .FirstOrDefault(a => a.AccountNumber == accountNumber && a.IsActive);

            if (account == null || account.Balance < amount) return false;

            var user = await _userManager.FindByIdAsync(account.ClientId);
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

            var sourceUser = await _userManager.FindByIdAsync(sourceAccount.ClientId);
            var destUser = await _userManager.FindByIdAsync(destAccount.ClientId);

            if (sourceUser == null || destUser == null) return false;

            // Actualizar balances
            sourceAccount.Balance -= amount;
            destAccount.Balance += amount;

            await _accountRepo.UpdateAsync(sourceAccount.Id, sourceAccount);
            await _accountRepo.UpdateAsync(destAccount.Id, destAccount);

            await _transactionService.RegisterTransactionAsync(
                savingsAccountId: sourceAccount.Id,
                type: "DÉBITO",
                amount: amount,
                origin: sourceAccount.AccountNumber,
                beneficiary: destAccount.AccountNumber,
                cashierId: cashierId
            );

            await _transactionService.RegisterTransactionAsync(
                savingsAccountId: destAccount.Id,
                type: "CRÉDITO",
                amount: amount,
                origin: sourceAccount.AccountNumber,
                beneficiary: destAccount.AccountNumber,
                cashierId: cashierId
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
                TotalPayments = allToday.Count(t =>
                    t.Origin.StartsWith("TC") || t.Origin.StartsWith("PRÉSTAMO")),
                Accounts = allAccounts
            };

            return dashboard;
        }

        public async Task<bool> MakeCreditCardPaymentAsync(string accountNumber, string cardNumber, decimal amount, string cashierId)
        {
            var cuenta = _accountRepo.GetAllQuery()
                .FirstOrDefault(a => a.AccountNumber == accountNumber && a.IsActive);

            var tarjeta = _creditCardRepo.GetAllQuery()
                .FirstOrDefault(t => t.CardNumber == cardNumber && t.IsActive);

            if (cuenta == null || tarjeta == null || cuenta.Balance < amount)
                return false;

            var user = await _userManager.FindByIdAsync(tarjeta.ClientId);
            if (user == null || string.IsNullOrEmpty(user.Email)) return false;

            decimal pagoReal = Math.Min(amount, tarjeta.Balance);

            // Debitar cuenta
            cuenta.Balance -= pagoReal;
            await _accountRepo.UpdateAsync(cuenta.Id, cuenta);

            // Reducir deuda tarjeta
            tarjeta.Balance -= pagoReal;
            await _creditCardRepo.UpdateAsync(tarjeta.Id, tarjeta);

            // Registrar transacción
            await _transactionService.RegisterTransactionAsync(
                savingsAccountId: cuenta.Id,
                type: "DÉBITO",
                amount: pagoReal,
                origin: cuenta.AccountNumber,
                beneficiary: tarjeta.CardNumber,
                cashierId: cashierId
            );

            // Email
            string last4Tarjeta = tarjeta.CardNumber[^4..];
            string last4Cuenta = cuenta.AccountNumber[^4..];
            string subject = $"Pago realizado a la tarjeta {last4Tarjeta}";

            string html = $@"
        <h3>Pago exitoso</h3>
        <p>Has pagado RD$ {pagoReal:N2} desde tu cuenta {cuenta.AccountNumber} a tu tarjeta {tarjeta.CardNumber}.</p>
        <ul>
            <li><strong>Fecha:</strong> {DateTime.Now:dd/MM/yyyy}</li>
            <li><strong>Hora:</strong> {DateTime.Now:hh:mm tt}</li>
        </ul>";

            await _emailService.SendAsync(new EmailRequestDto
            {
                To = user.Email,
                Subject = subject,
                HtmlBody = html
            });

            return true;
        }

        public async Task<(SavingsAccount? account, string? clientFullName)> GetAccountWithClientNameAsync(string accountNumber)
        {
            var account = _accountRepo
                .GetAllQuery()
                .FirstOrDefault(a => a.AccountNumber == accountNumber && a.IsActive);

            if (account == null) return (null, null);

            var user = await _userManager.FindByIdAsync(account.ClientId);

            if (user == null) return (account, null);

            string fullName = $"{user.Name} {user.LastName}";

            return (account, fullName);
        }

        public async Task<bool> MakeLoanPaymentAsync(string accountNumber, string loanNumber, decimal amount, string cashierId)
        {
            var cuenta = _accountRepo.GetAllQuery()
                .FirstOrDefault(a => a.AccountNumber == accountNumber && a.IsActive);

            var prestamo = _loanRepo.GetAllQueryWithInclude(["Installments"])
                .FirstOrDefault(p => p.LoanNumber == loanNumber && !p.IsCompleted);

            if (cuenta == null || prestamo == null || cuenta.Balance < amount)
                return false;

            var cliente = await _userManager.FindByIdAsync(prestamo.ClientId);
            if (cliente == null || string.IsNullOrEmpty(cliente.Email)) return false;

            decimal restante = amount;

            // Ordenar cuotas pendientes
            var cuotas = prestamo.Installments
                .Where(c => !c.IsPaid)
                .OrderBy(c => c.DueDate)
                .ToList();

            foreach (var cuota in cuotas)
            {
                decimal faltante = cuota.Amount - cuota.AmountPaid;
                if (restante <= 0) break;

                if (restante >= faltante)
                {
                    cuota.AmountPaid += faltante;
                    restante -= faltante;
                }
                else
                {
                    cuota.AmountPaid += restante;
                    restante = 0;
                }

                await _installmentRepo.UpdateAsync(cuota.Id, cuota);
            }

            // Actualizar saldo de cuenta y préstamo
            decimal pagado = amount - restante;
            cuenta.Balance -= pagado;
            await _accountRepo.UpdateAsync(cuenta.Id, cuenta);

            prestamo.RemainingAmount -= pagado;
            prestamo.IsCompleted = prestamo.RemainingAmount <= 0;
            await _loanRepo.UpdateAsync(prestamo.Id, prestamo);

            // Si sobró dinero, devolverlo a cuenta
            if (restante > 0)
            {
                cuenta.Balance += restante;
                await _accountRepo.UpdateAsync(cuenta.Id, cuenta);
            }

            // Registrar transacción
            await _transactionService.RegisterTransactionAsync(
                savingsAccountId: cuenta.Id,
                type: "DÉBITO",
                amount: pagado,
                origin: cuenta.AccountNumber,
                beneficiary: loanNumber,
                cashierId: cashierId
            );

            // Email
            string last4Cuenta = cuenta.AccountNumber[^4..];
            string subject = $"Pago realizado al préstamo {loanNumber}";

            string htmlBody = $@"
        <h3>Pago aplicado</h3>
        <p>Has realizado un pago al préstamo <strong>{loanNumber}</strong>.</p>
        <ul>
            <li><strong>Monto:</strong> RD$ {pagado:N2}</li>
            <li><strong>Cuenta usada:</strong> {cuenta.AccountNumber}</li>
            <li><strong>Fecha:</strong> {DateTime.Now:dd/MM/yyyy}</li>
            <li><strong>Hora:</strong> {DateTime.Now:hh:mm tt}</li>
        </ul>";

            await _emailService.SendAsync(new EmailRequestDto
            {
                To = cliente.Email,
                Subject = subject,
                HtmlBody = htmlBody
            });

            return true;
        }

        public async Task<(Loan? loan, string clientFullName, decimal remainingDebt)> GetLoanInfoAsync(string loanNumber)
        {
            var prestamo = _loanRepo.GetAllQuery().FirstOrDefault(p => p.LoanNumber == loanNumber && !p.IsCompleted);

            if (prestamo == null)
                return (null, "", 0);

            var cliente = await _userManager.FindByIdAsync(prestamo.ClientId);
            string nombreCompleto = cliente != null ? $"{cliente.Name} {cliente.LastName}" : "";

            return (prestamo, nombreCompleto, prestamo.RemainingAmount);
        }

        public List<SavingsAccount> GetAllActiveAccounts()
        {
            return _accountRepo.GetAllQuery().Where(a => a.IsActive).ToList();
        }

    }
}
