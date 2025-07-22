using Microsoft.AspNetCore.Mvc;
using HermesBanking.Core.Application.Interfaces; // Asegúrate de que este namespace sea correcto
using HermesBanking.Core.Domain.Common.Enums; // Para Roles enum
using HermesBanking.Core.Application.ViewModels.HomeAdmin;
using System.Threading.Tasks; // Para Task
using System.Linq; // Para Count() y Where()
using System; // Para DateTime.Today

// Agrega este namespace si no está para TransactionDTO, aunque no lo usaremos directamente aquí
// using HermesBanking.Core.Application.DTOs.Transaction;
// using System.Collections.Generic; // Para List, si fuera necesario directamente

namespace HermesBankingApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ICreditCardService _creditCardService;
        private readonly ISavingsAccountService _savingsAccountService;
        private readonly ILoanService _loanService;
        private readonly IAccountServiceForWebApp _accountServiceForWebApp;
        private readonly ITransactionService _transactionService; // ¡Inyectar ITransactionService!

        public HomeController(
            ICreditCardService creditCardService,
            ISavingsAccountService savingsAccountService,
            ILoanService loanService,
            IAccountServiceForWebApp accountServiceForWebApp,
            ITransactionService transactionService) // ¡Asegúrate de agregarlo al constructor!
        {
            _creditCardService = creditCardService;
            _savingsAccountService = savingsAccountService;
            _loanService = loanService;
            _accountServiceForWebApp = accountServiceForWebApp;
            _transactionService = transactionService; // ¡Inicializar!
        }

        public async Task<IActionResult> Index()
        {
            var viewModel = new DashboardViewModel();

            // Fetch credit card data (existing logic - NO CAMBIOS)
            var allCreditCardsResponse = await _creditCardService.GetCreditCardsAsync();
            viewModel.TotalIssuedCreditCards = allCreditCardsResponse.Paginacion.TotalRegistros;
            viewModel.ActiveCreditCards = allCreditCardsResponse.Data.Count(c => c.IsActive);
            viewModel.InactiveCreditCards = allCreditCardsResponse.Data.Count(c => !c.IsActive);

            // Fetch savings account data (existing logic - NO CAMBIOS)
            var allSavingsAccounts = await _savingsAccountService.GetAllSavingsAccountsOfClients();
            viewModel.TotalSavingsAccounts = allSavingsAccounts.Count;

            // Fetch loan data (existing logic - NO CAMBIOS)
            var allLoans = await _loanService.GetAllLoansAsync(null, "activos");
            viewModel.ActiveLoans = allLoans.Count;
            viewModel.AverageClientDebt = await _loanService.GetAverageSystemDebt();

            // Fetch client data (existing logic - NO CAMBIOS)
            var allClients = await _accountServiceForWebApp.GetAllUserByRole(Roles.Client.ToString());
            viewModel.ActiveClients = allClients.Count(c => c.IsActive);
            viewModel.InactiveClients = allClients.Count(c => !c.IsActive);

            // --- NUEVA LÓGICA: Fetch y cálculo de transacciones para el dashboard ---
            var allTransactions = await _transactionService.GetAllTransactionsAsync();
            viewModel.TotalTransactions = allTransactions.Count; // Total de transacciones
            viewModel.TransactionsToday = allTransactions.Count(t => t.TransactionDate.Date == DateTime.Today); // Transacciones de hoy

          
            viewModel.TotalPaymentsAmount = allTransactions
                .Where(t => t.TransactionType == TransactionType.PagoTarjetaCredito || t.TransactionType == TransactionType.PagoPrestamo || t.TransactionType == TransactionType.PagoBeneficiario)
                .Sum(t => t.Amount);

            viewModel.PaymentsProcessedToday = allTransactions
                .Where(t => t.TransactionDate.Date == DateTime.Today &&
                            (t.TransactionType == TransactionType.PagoTarjetaCredito || t.TransactionType == TransactionType.PagoPrestamo || t.TransactionType == TransactionType.PagoBeneficiario))
                .Sum(t => t.Amount);
            // --- FIN DE NUEVA LÓGICA ---

            return View(viewModel);
        }
    }
}