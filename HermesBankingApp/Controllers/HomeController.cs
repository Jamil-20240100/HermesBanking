using Microsoft.AspNetCore.Mvc;
using HermesBanking.Core.Application.Interfaces; // Ensure this namespace is correct for your services
using HermesBanking.Core.Domain.Common.Enums; // For Roles enum
using HermesBanking.Core.Application.ViewModels.HomeAdmin;

namespace HermesBankingApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ICreditCardService _creditCardService;
        private readonly ISavingsAccountService _savingsAccountService;
        private readonly ILoanService _loanService;
        private readonly IAccountServiceForWebApp _accountServiceForWebApp; // Inject the account service

        public HomeController(
            ICreditCardService creditCardService,
            ISavingsAccountService savingsAccountService,
            ILoanService loanService,
            IAccountServiceForWebApp accountServiceForWebApp) // Add IAccountServiceForWebApp to the constructor
        {
            _creditCardService = creditCardService;
            _savingsAccountService = savingsAccountService;
            _loanService = loanService;
            _accountServiceForWebApp = accountServiceForWebApp; // Initialize the account service
        }

        public async Task<IActionResult> Index()
        {
            var viewModel = new DashboardViewModel();

            // Fetch credit card data
            var allCreditCardsResponse = await _creditCardService.GetCreditCardsAsync();
            viewModel.TotalIssuedCreditCards = allCreditCardsResponse.Paginacion.TotalRegistros;
            viewModel.ActiveCreditCards = allCreditCardsResponse.Data.Count(c => c.IsActive);
            viewModel.InactiveCreditCards = allCreditCardsResponse.Data.Count(c => !c.IsActive);

            // Fetch savings account data
            var allSavingsAccounts = await _savingsAccountService.GetAllSavingsAccountsOfClients();
            viewModel.TotalSavingsAccounts = allSavingsAccounts.Count;

            // Fetch loan data
            var allLoans = await _loanService.GetAllLoansAsync(null, "activos");
            viewModel.ActiveLoans = allLoans.Count;
            viewModel.AverageClientDebt = await _loanService.GetAverageSystemDebt();

            // Fetch client data
            var allClients = await _accountServiceForWebApp.GetAllUserByRole(Roles.Client.ToString());
            viewModel.ActiveClients = allClients.Count(c => c.IsActive);
            viewModel.InactiveClients = allClients.Count(c => !c.IsActive);

            return View(viewModel);
        }
    }
}