using AutoMapper;
using HermesBanking.Core.Application.DTOs.SavingsAccount;
using HermesBanking.Core.Application.Interfaces;
using HermesBanking.Core.Application.Services;
using HermesBanking.Core.Application.ViewModels.SavingsAccount;
using HermesBanking.Core.Application.ViewModels.User;
using HermesBanking.Core.Domain.Common.Enums;
using HermesBanking.Infrastructure.Identity.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace HermesBankingApp.Controllers
{
    [Authorize(Roles = "Admin")]
    public class SavingsAccountController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IAccountServiceForWebApp _accountServiceForWebApp;
        private readonly ISavingsAccountService _service;
        private readonly IMapper _mapper;
        private readonly ILoanService _loanService;

        public SavingsAccountController(ISavingsAccountService service, IMapper mapper, UserManager<AppUser> userManager, IAccountServiceForWebApp accountServiceForWebApp, ILoanService loanService)
        {
            _service = service;
            _mapper = mapper;
            _userManager = userManager;
            _accountServiceForWebApp = accountServiceForWebApp;
            _loanService = loanService;
        }

        public async Task<IActionResult> Index()
        {
            //
            //session verification
            //
            AppUser? userSession = await _userManager.GetUserAsync(User);
            if (userSession == null)
                return RedirectToRoute(new { controller = "Login", action = "Index" });

            var user = await _accountServiceForWebApp.GetUserByUserName(userSession.UserName ?? "");
            if (user == null)
                return RedirectToRoute(new { controller = "Login", action = "Index" });
            //
            //
            //

            //obtaining Savings Accounts' information
            var DTOsList = await _service.GetAllSavingsAccountsOfClients();
            var VMsList = _mapper.Map<List<SavingsAccountViewModel>>(DTOsList);

            return View(VMsList);
        }

        public async Task<IActionResult> Create()
        {
            //
            //session verification
            //
            AppUser? userSession = await _userManager.GetUserAsync(User);
            if (userSession == null)
                return RedirectToRoute(new { controller = "Login", action = "Index" });

            var user = await _accountServiceForWebApp.GetUserByUserName(userSession.UserName ?? "");
            if (user == null)
                return RedirectToRoute(new { controller = "Login", action = "Index" });

            //
            // Get active clients and assign debt
            //
            var clients = await _accountServiceForWebApp.GetAllActiveUserByRole(Roles.Client.ToString());

            foreach (var client in clients)
            {
                client.TotalDebt = await _loanService.GetCurrentDebtForClient(client.Id);
            }

            // Map the already enriched list
            var activeClientsVMs = _mapper.Map<List<UserViewModel>>(clients);

            return View("Save", activeClientsVMs);
        }


        [HttpPost]
        public async Task<IActionResult> Create(SaveSavingsAccountViewModel vm)
        {
            //
            //session verification
            //
            AppUser? userSession = await _userManager.GetUserAsync(User);
            if (userSession == null)
                return RedirectToRoute(new { controller = "Login", action = "Index" });

            var user = await _accountServiceForWebApp.GetUserByUserName(userSession.UserName ?? "");
            if (user == null)
                return RedirectToRoute(new { controller = "Login", action = "Index" });
            //
            //
            //
            vm.AccountNumber = await _service.GenerateUniqueAccountNumberAsync();
            vm.CreatedByAdminId = userSession.Id;
            vm.AdminFullName = $"{userSession.Name} {userSession.LastName}";

            if (!ModelState.IsValid)
                return RedirectToRoute(new { controller = "SavingsAccount", action = "Index" });

            var dto = _mapper.Map<SavingsAccountDTO>(vm);
            await _service.AddAsync(dto);

            return RedirectToRoute(new { controller = "SavingsAccount", action = "Index" });
        }

        public async Task<IActionResult> CreateSecondaryForm(string clientId)
        {
            //
            //session verification
            //
            AppUser? userSession = await _userManager.GetUserAsync(User);
            if (userSession == null)
                return RedirectToRoute(new { controller = "Login", action = "Index" });

            var user = await _accountServiceForWebApp.GetUserByUserName(userSession.UserName ?? "");
            if (user == null)
                return RedirectToRoute(new { controller = "Login", action = "Index" });
            //
            //
            //
            var userDTO = await _accountServiceForWebApp.GetUserById(clientId); //obtain client

            var vm = new SaveSavingsAccountViewModel
            {
                Id = 0,
                AccountNumber = "",
                ClientId = userDTO.Id,
                ClientFullName = $"{userDTO.Name} {userDTO.LastName}",
                AccountType = AccountType.Secondary,
                CreatedAt = DateTime.Now,
                IsActive = true,
                Balance = 0,
            };


            return View("CreateSecondaryForm", vm);
        }

        public async Task<IActionResult> ConfirmCancel(int id)
        {
            //
            //session verification
            //
            AppUser? userSession = await _userManager.GetUserAsync(User);
            if (userSession == null)
                return RedirectToRoute(new { controller = "Login", action = "Index" });

            var user = await _accountServiceForWebApp.GetUserByUserName(userSession.UserName ?? "");
            if (user == null)
                return RedirectToRoute(new { controller = "Login", action = "Index" });
            //
            //
            //

            var account = _service.GetById(id);

            var vm = new CancelSavingsAccountViewModel
            {
                Id = account.Id,
            };

            return View("Cancel", vm);
        }

        [HttpPost]
        public async Task<IActionResult> Cancel(int id)
        {
            try
            {
                await _service.TransferBalanceAndCancelAsync(id);
                TempData["Success"] = "La cuenta fue cancelada correctamente y el balance fue transferido.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction("Index");
        }

    }
}