using AutoMapper;
using HermesBanking.Core.Application.DTOs.Transfer;
using HermesBanking.Core.Application.Interfaces;
using HermesBanking.Core.Application.ViewModels.SavingsAccount;
using HermesBanking.Core.Application.ViewModels.Transfer;
using HermesBanking.Infrastructure.Identity.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace HermesBankingApp.Controllers
{
    public class TransferController : Controller
    {
        private readonly ITransferService _transferService;
        private readonly ISavingsAccountService _savingsAccountService;
        private readonly UserManager<AppUser> _userManager;
        private readonly IMapper _mapper;

        public TransferController(
            ITransferService transferService,
            ISavingsAccountService savingsAccountService,
            UserManager<AppUser> userManager,
            IMapper mapper)
        {
            _transferService = transferService;
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

            var vm = await BuildTransferViewModel(user.Id);
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(TransferViewModel vm)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Index", "Login");
            }

            if (vm.SourceAccountId == vm.DestinationAccountId)
            {
                ModelState.AddModelError("DestinationAccountId", "La cuenta de destino no puede ser la misma que la de origen.");
            }

            if (!ModelState.IsValid)
            {
                var freshVm = await BuildTransferViewModel(user.Id);
                vm.AvailableAccounts = freshVm.AvailableAccounts;
                return View(vm);
            }

            var dto = _mapper.Map<TransferDTO>(vm);

            var result = await _transferService.ProcessTransferAsync(dto);

            if (result.HasError)
            {
                var freshVm = await BuildTransferViewModel(user.Id);
                freshVm.HasError = true;
                freshVm.ErrorMessage = result.ErrorMessage;
                return View(freshVm);
            }

            return RedirectToAction("Index", "ClientHome");
        }

        private async Task<TransferViewModel> BuildTransferViewModel(string clientId)
        {
            var allAccounts = await _savingsAccountService.GetAll();
            var clientAccounts = allAccounts
                .Where(a => a.ClientId == clientId && a.IsActive)
                .ToList();

            var vm = new TransferViewModel
            {
                AvailableAccounts = _mapper.Map<List<SavingsAccountViewModel>>(clientAccounts)
            };
            return vm;
        }
    }
}