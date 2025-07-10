using HermesBanking.Core.Application.DTOs.SavingsAccount;
using HermesBanking.Core.Application.DTOs.User;
using HermesBanking.Core.Application.Interfaces;
using HermesBanking.Core.Application.ViewModels.User;
using HermesBanking.Core.Domain.Common.Enums;
using HermesBankingApp.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.Design;

namespace HermesBankingApp.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UserController : Controller
    {
        private readonly IAccountServiceForWebApp _accountServiceForWebApp;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ISavingsAccountService _savingsAccountService;

        public UserController(IAccountServiceForWebApp accountServiceForWebApp, RoleManager<IdentityRole> roleManager, ISavingsAccountService savingsAccountService)
        {
            _accountServiceForWebApp = accountServiceForWebApp;
            _roleManager = roleManager;
            _savingsAccountService = savingsAccountService;
        }
        public async Task<IActionResult> Index()
        {
            var DTOs = await _accountServiceForWebApp.GetAllUser(false);

            var listEntityVms = DTOs.Select(s =>
              new UserViewModel()
              {
                  Id = s.Id,
                  Name = s.Name,
                  Email = s.Email,
                  UserName = s.UserName,
                  LastName = s.LastName,
                  Role = s.Role,
                  InitialAmount = s.InitialAmount,
                  UserId = s.UserId,
                  IsActive = s.IsActive,
              }).ToList();

            return View(listEntityVms);
        }
        
        public async Task<IActionResult> Create()
        {
            ViewBag.Roles = await _roleManager.Roles.ToListAsync();
            return View(new CreateUserViewModel() { Id = 0, Name = "", Email = "", UserName = "", LastName = "", Password = "", ConfirmPassword = "", Role = "", UserId = "", IsActive = true, InitialAmount = 0});
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateUserViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Roles = await _roleManager.Roles.ToListAsync();
                return View(vm);
            }

            string origin = Request?.Headers?.Origin.ToString() ?? string.Empty;

            SaveUserDto dto = new()
            {
                Id = "",
                Name = vm.Name,
                Email = vm.Email,
                UserName = vm.UserName,
                LastName = vm.LastName,
                Password = vm.Password,
                Role = vm.Role,
                UserId = vm.UserId,
                InitialAmount = vm.InitialAmount,
                IsActive = vm.IsActive
            };

            RegisterResponseDto? returnUser = await _accountServiceForWebApp.RegisterUser(dto, origin);

            if (returnUser.HasError)
            {
                ViewBag.Roles = await _roleManager.Roles.ToListAsync();
                ViewBag.HasError = true;
                ViewBag.Errors = returnUser.Errors;
                return View(vm);
            }

            if (returnUser != null && !string.IsNullOrWhiteSpace(returnUser.Id))
            {
                dto.Id = returnUser.Id;
                await _accountServiceForWebApp.EditUser(dto, origin, true);
            }

            //
            // ADDING NEW PRIMARY ACCOUNT
            //

            if(dto.Role == Roles.Client.ToString())
            {
                var newAccount = new SavingsAccountDTO
                {
                    Id = 0,
                    IsActive = true,
                    AccountNumber = "000000000", //insertar logica para crear 9 digitos
                    AccountType = AccountType.Primary,
                    Balance = dto.InitialAmount ?? 0,
                    ClientFullName = $"{dto.Name} {dto.LastName}",
                    ClientId = dto.Id,
                    CreatedAt = DateTime.Now,
                };
                
                await _savingsAccountService.AddAsync(newAccount);
            }

            //
            //
            //

            return RedirectToRoute(new { controller = "User", action = "Index" });
        }

        public async Task<IActionResult> Edit(string id)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToRoute(new { controller = "User", action = "Index" });
            }

            ViewBag.EditMode = true;
            var dto = await _accountServiceForWebApp.GetUserById(id);

            if (dto == null)
            {
                return RedirectToRoute(new { controller = "User", action = "Index" });
            }

            UpdateUserViewModel vm = new()
            {
                Id = dto.Id,
                Name = dto.Name,
                Email = dto.Email,
                UserName = dto.UserName,
                LastName = dto.LastName,
                Password = "",
                Role = dto.Role,
                UserId = dto.UserId,
                InitialAmount = dto.InitialAmount,
                IsActive = dto.IsActive,
            };

            ViewBag.Roles = await _roleManager.Roles.ToListAsync();
            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(UpdateUserViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Roles = await _roleManager.Roles.ToListAsync();
                ViewBag.EditMode = true;
                return View(vm);
            }

            string origin = Request?.Headers?.Origin.ToString() ?? string.Empty;

            SaveUserDto dto = new()
            {
                Id = vm.Id,
                Name = vm.Name,
                Email = vm.Email,
                UserName = vm.UserName,
                LastName = vm.LastName,
                Password = vm.Password ?? "",
                Role = vm.Role,
                InitialAmount= vm.InitialAmount,
                UserId= vm.UserId,
                IsActive = vm.IsActive
            };

            var currentDto = await _accountServiceForWebApp.GetUserById(vm.Id);

            var returnUser = await _accountServiceForWebApp.EditUser(dto, origin);
            if (returnUser.HasError)
            {
                ViewBag.Roles = await _roleManager.Roles.ToListAsync();
                ViewBag.EditMode = true;
                ViewBag.HasError = true;
                ViewBag.Errors = returnUser.Errors;
                return View(vm);
            }

            return RedirectToRoute(new { controller = "User", action = "Index" });
        }

        public async Task<IActionResult> Delete(string id)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToRoute(new { controller = "User", action = "Index" });
            }

            var dto = await _accountServiceForWebApp.GetUserById(id);
            if (dto == null)
            {
                return RedirectToRoute(new { controller = "User", action = "Index" });
            }
            DeleteUserViewModel vm = new() { Id = dto.Id, Name = dto.Name, LastName = dto.LastName };
            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(DeleteUserViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                return View(vm);
            }

            await _accountServiceForWebApp.DeleteAsync(vm.Id);
            FileManager.Delete(vm.Id, "Users");
            return RedirectToRoute(new { controller = "User", action = "Index" });
        }

        public async Task<IActionResult> Toggle(string id)
        {
            var dto = await _accountServiceForWebApp.GetUserById(id);
         
            if (dto == null) return RedirectToAction("Index");

            var vm = new ToggleUserStateViewModel
            {
                Id = dto.Id,
                Name = dto.Name,
                LastName = dto.LastName,
                Email = dto.Email,
                UserName = dto.UserName,
                IsActive = dto.IsActive,
                Role = dto.Role,
                InitialAmount = dto.InitialAmount,
                UserId = dto.UserId,
            };

            return View("ConfirmAction",vm);
        }

        [HttpPost]
        public async Task<IActionResult> Toggle(ToggleUserStateViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Roles = await _roleManager.Roles.ToListAsync();
                ViewBag.EditMode = true;
                return View("ConfirmAction", vm);
            }

            string origin = Request?.Headers?.Origin.ToString() ?? string.Empty;

            var newState = true;
            if(vm.IsActive == newState)
            {
                newState = false;
            }
            else
            {
                newState = true;
            }

            SaveUserDto dto = new()
            {
                Id = vm.Id,
                Name = vm.Name ?? "",
                LastName = vm.LastName ?? "",
                Email = vm.Email ?? "",
                UserName = vm.UserName ?? "",
                Role = vm.Role ?? "",
                IsActive = newState,
                InitialAmount = vm.InitialAmount,
                Password = vm.Password ?? "",
                UserId = vm.UserId ?? "",
                
            };

            //var currentDto = await _accountServiceForWebApp.GetUserById(dto.Id);

            var returnUser = await _accountServiceForWebApp.EditUser(dto, origin);
            if (returnUser.HasError)
            {
                ViewBag.Roles = await _roleManager.Roles.ToListAsync();
                ViewBag.EditMode = true;
                ViewBag.HasError = true;
                ViewBag.Errors = returnUser.Errors;
                return View(vm);
            }

            return RedirectToRoute(new { controller = "User", action = "Index" });
        }
    }
}
