using AutoMapper;
using HermesBanking.Core.Application.Interfaces;
using HermesBanking.Core.Application.ViewModels.Beneficiary;
using HermesBanking.Infrastructure.Identity.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Linq;
using HermesBanking.Core.Application.DTOs.Beneficiary;

namespace HermesBankingApp.Controllers
{
    public class BeneficiaryController : Controller
    {
        private readonly IBeneficiaryService _beneficiaryService;
        private readonly ISavingsAccountService _savingsAccountService;
        private readonly UserManager<AppUser> _userManager;
        private readonly IMapper _mapper;

        public BeneficiaryController(
            IBeneficiaryService beneficiaryService,
            ISavingsAccountService savingsAccountService,
            UserManager<AppUser> userManager,
            IMapper mapper)
        {
            _beneficiaryService = beneficiaryService;
            _savingsAccountService = savingsAccountService;
            _userManager = userManager;
            _mapper = mapper;
        }

        public async Task<IActionResult> Index()
        {
            AppUser? user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToRoute(new { controller = "Login", action = "Index" });
            }

            var beneficiaryDtos = await _beneficiaryService.GetAllByClientIdAsync(user.Id);
            var viewModels = _mapper.Map<List<BeneficiaryViewModel>>(beneficiaryDtos);

            return View(viewModels);
        }

        [HttpPost]
        public async Task<IActionResult> Add(SaveBeneficiaryViewModel vm)
        {
            AppUser? user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToRoute(new { controller = "Login", action = "Index" });
            }

            if (!ModelState.IsValid)
            {
                vm.HasError = true;
                vm.ErrorMessage = "El número de cuenta no puede estar vacío.";
                return View("Index", await GetBeneficiaryListWithViewModel(vm));
            }
            
            var targetAccount = await _savingsAccountService.GetByAccountNumberAsync(vm.BeneficiaryAccountNumber);
            if (targetAccount == null)
            {
                vm.HasError = true;
                vm.ErrorMessage = "El número de cuenta ingresado no corresponde a ninguna cuenta válida.";
                return View("Index", await GetBeneficiaryListWithViewModel(vm));
            }
            
            if (targetAccount.ClientId == user.Id)
            {
                vm.HasError = true;
                vm.ErrorMessage = "No puedes agregarte a ti mismo como beneficiario.";
                return View("Index", await GetBeneficiaryListWithViewModel(vm));
            }
            
            var existingBeneficiaries = await _beneficiaryService.GetAllByClientIdAsync(user.Id);
            if (existingBeneficiaries.Any(b => b.BeneficiaryAccountNumber == vm.BeneficiaryAccountNumber))
            {
                vm.HasError = true;
                vm.ErrorMessage = "Esta cuenta ya está registrada como beneficiario.";
                return View("Index", await GetBeneficiaryListWithViewModel(vm));
            }

            var dto = new BeneficiaryDTO
            {
                ClientId = user.Id,
                BeneficiaryAccountNumber = targetAccount.AccountNumber,
                Name = targetAccount.ClientFullName,
                LastName = targetAccount.ClientFullName,
                CreatedAt = DateTime.Now
            };

            await _beneficiaryService.AddAsync(dto);
            return RedirectToAction("Index");
        }
        
        public async Task<IActionResult> Delete(int id)
        {
            var beneficiary = await _beneficiaryService.GetById(id);
            if (beneficiary == null)
            {
                return NotFound();
            }
            var vm = _mapper.Map<DeleteBeneficiaryViewModel>(beneficiary);
            return View("ConfirmDelete", vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeletePost(int id)
        {
            await _beneficiaryService.DeleteAsync(id);
            return RedirectToAction("Index");
        }
        
        private async Task<List<BeneficiaryViewModel>> GetBeneficiaryListWithViewModel(SaveBeneficiaryViewModel vm)
        {
            AppUser? user = await _userManager.GetUserAsync(User);
            var beneficiaryDtos = await _beneficiaryService.GetAllByClientIdAsync(user.Id);
            var viewModels = _mapper.Map<List<BeneficiaryViewModel>>(beneficiaryDtos);
            ViewBag.SaveViewModel = vm;
            return viewModels;
        }
    }
}