using AutoMapper;
using HermesBanking.Core.Application.DTOs.SavingsAccount;
using HermesBanking.Core.Application.Interfaces;
using HermesBanking.Core.Application.ViewModels.SavingsAccount;
using HermesBanking.Core.Application.ViewModels.User;
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
        
        public SavingsAccountController(ISavingsAccountService service, IMapper mapper, UserManager<AppUser> userManager, IAccountServiceForWebApp accountServiceForWebApp)
        {
            _service = service;
            _mapper = mapper;
            _userManager = userManager;
            _accountServiceForWebApp = accountServiceForWebApp;
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
            //
            //

            var activeClientsDTOs = _accountServiceForWebApp.GetAllUser(true);
            var activeClientsVMs = _mapper.Map<List<UserViewModel>>(activeClientsDTOs);

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

            vm.AdminFullName = $"{userSession.Name} {userSession.LastName}";

            if (!ModelState.IsValid)
                return RedirectToRoute(new { controller = "SavingsAccount", action = "Index" });

            var dto = _mapper.Map<SavingsAccountDTO>(vm);
            await _service.AddAsync(dto);

            return RedirectToRoute(new { controller = "SavingsAccount", action = "Index" });
        }

        //
        // FALTA VERIFICAR SI LA CUENTA SECUNDARIA TIENE BALANCE. SI TIENE, SE TRANSFIERE A LA PRINCIPAL
        //

        public async Task<IActionResult> ConfirmDelete(int id)
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

            var vm = new DeleteSavingsAccountViewModel
            {
                Id = account.Id,
            };
            
            return View("Delete", vm);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            await _service.DeleteAsync(id);
            return RedirectToAction("Index");
        }
    }
}
