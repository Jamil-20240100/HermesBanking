﻿using AutoMapper;
using HermesBanking.Core.Application.DTOs.CashAdvance;
using HermesBanking.Core.Application.Interfaces;
using HermesBanking.Core.Application.ViewModels.CashAdvance;
using HermesBanking.Core.Application.ViewModels.CreditCard;
using HermesBanking.Core.Application.ViewModels.SavingsAccount;
using HermesBanking.Infrastructure.Identity.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace HermesBankingApp.Controllers
{
    public class CashAdvanceController : Controller
    {
        private readonly ICashAdvanceService _cashAdvanceService;
        private readonly ICreditCardService _creditCardService;
        private readonly ISavingsAccountService _savingsAccountService;
        private readonly UserManager<AppUser> _userManager;
        private readonly IMapper _mapper;

        public CashAdvanceController(
            ICashAdvanceService cashAdvanceService,
            ICreditCardService creditCardService,
            ISavingsAccountService savingsAccountService,
            UserManager<AppUser> userManager,
            IMapper mapper)
        {
            _cashAdvanceService = cashAdvanceService;
            _creditCardService = creditCardService;
            _savingsAccountService = savingsAccountService;
            _userManager = userManager;
            _mapper = mapper;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Index", "Login");
            }

            var vm = await BuildCashAdvanceViewModel(user.Id);
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(CashAdvanceViewModel vm)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Index", "Login");
            }

            if (!ModelState.IsValid)
            {
                var freshVm = await BuildCashAdvanceViewModel(user.Id);
                vm.AvailableCreditCards = freshVm.AvailableCreditCards;
                vm.AvailableSavingsAccounts = freshVm.AvailableSavingsAccounts;
                return View(vm);
            }

            var dto = new CashAdvanceDTO
            {
                SourceCreditCardId = vm.SourceCreditCardId,
                DestinationSavingsAccountId = vm.DestinationSavingsAccountId,
                Amount = vm.Amount,
                ClientId = user.Id
            };

            var result = await _cashAdvanceService.ProcessCashAdvanceAsync(dto);

            if (result.HasError)
            {
                var freshVm = await BuildCashAdvanceViewModel(user.Id);
                freshVm.HasError = true;
                freshVm.ErrorMessage = result.ErrorMessage;
                return View(freshVm);
            }

            return RedirectToAction("Index", "ClientHome");
        }

        private async Task<CashAdvanceViewModel> BuildCashAdvanceViewModel(string clientId)
        {
            var allCards = await _creditCardService.GetAll();
            var clientCreditCards = allCards
                .Where(c => c.ClientId == clientId && c.IsActive)
                .ToList();

            var allAccounts = await _savingsAccountService.GetAll();
            var clientSavingsAccounts = allAccounts
                .Where(a => a.ClientId == clientId && a.IsActive)
                .ToList();

            var vm = new CashAdvanceViewModel
            {
                AvailableCreditCards = _mapper.Map<List<CreditCardViewModel>>(clientCreditCards),
                AvailableSavingsAccounts = _mapper.Map<List<SavingsAccountViewModel>>(clientSavingsAccounts)
            };

            return vm;
        }
    }
}