// src/HermesBankingApp/Controllers/LoanController.cs

using Microsoft.AspNetCore.Mvc;
using HermesBanking.Core.Application.Interfaces;
using HermesBanking.Core.Application.DTOs.Loan;
using HermesBanking.Core.Application.ViewModels.Loan;
using Microsoft.AspNetCore.Authorization;
using AutoMapper;
using HermesBanking.Core.Domain.Common.Enums;
using Microsoft.AspNetCore.Identity;
using HermesBanking.Infrastructure.Identity.Entities;
using HermesBanking.Core.Application.ViewModels.User;

namespace HermesBankingApp.Controllers
{
    [Authorize]
    public class LoanController : Controller
    {
        private readonly ILoanService _loanService;
        private readonly IMapper _mapper;
        private readonly IAccountServiceForWebApp _accountServiceForWebApp;
        private readonly UserManager<AppUser> _userManager;

        public LoanController(ILoanService loanService, IMapper mapper, IAccountServiceForWebApp accountServiceForWebApp, UserManager<AppUser> userManager)
        {
            _loanService = loanService;
            _mapper = mapper;
            _accountServiceForWebApp = accountServiceForWebApp;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index([FromQuery] string? cedula, [FromQuery] string? status = "all")
        {
            var loanDTOs = await _loanService.GetAllLoansAsync(cedula, status);
            var loanViewModels = _mapper.Map<List<LoanViewModel>>(loanDTOs);
            ViewBag.CedulaFilter = cedula;
            ViewBag.StatusFilter = status;

            if (TempData["SuccessMessage"] != null)
            {
                ViewBag.SuccessMessage = TempData["SuccessMessage"];
            }
            if (TempData["ErrorMessage"] != null) // Asegúrate de leer este TempData también en Index
            {
                ViewBag.ErrorMessage = TempData["ErrorMessage"];
            }
            return View(loanViewModels);
        }

        public async Task<IActionResult> Details(int id)
        {
            var loanDTO = await _loanService.GetLoanByIdAsync(id);
            if (loanDTO == null)
            {
                return NotFound();
            }

            var amortizationDTOs = await _loanService.GetAmortizationTableByLoanIdAsync(id);
            var amortizationViewModels = _mapper.Map<List<AmortizationInstallmentViewModel>>(amortizationDTOs);
            var viewModel = _mapper.Map<LoanDetailsViewModel>(loanDTO);
            viewModel.AmortizationSchedule = amortizationViewModels;

            return View(viewModel);
        }

        public async Task<IActionResult> AssignLoan()
        {
            AppUser? userSession = await _userManager.GetUserAsync(User);
            if (userSession == null)
                return RedirectToRoute(new { controller = "Login", action = "Index" });

            var adminUser = await _accountServiceForWebApp.GetUserByUserName(userSession.UserName ?? "");
            if (adminUser == null)
                return RedirectToRoute(new { controller = "Login", action = "Index" });

            var userDTOs = await _accountServiceForWebApp.GetAllActiveUserByRole(Roles.Client.ToString());

            var clientsForDropdown = (userDTOs != null) ? _mapper.Map<List<UserViewModel>>(userDTOs) : new List<UserViewModel>();

            var pageViewModel = new AssignLoanPageViewModel
            {
                LoanData = new AssignLoanViewModel(),
                Clients = clientsForDropdown
            };

            return View(pageViewModel);
        }

        [HttpPost]
        public async Task<IActionResult> AssignLoan(AssignLoanPageViewModel pageViewModel)
        {
            AppUser? userSession = await _userManager.GetUserAsync(User);
            if (userSession == null)
                return RedirectToRoute(new { controller = "Login", action = "Index" });

            var adminUser = await _accountServiceForWebApp.GetUserByUserName(userSession.UserName ?? "");
            if (adminUser == null)
                return RedirectToRoute(new { controller = "Login", action = "Index" });

            var loanModel = pageViewModel.LoanData;

            loanModel.CreatedByAdminId = userSession.Id;
            loanModel.AdminFullName = $"{adminUser.Name} {adminUser.LastName}";
            if (loanModel.InterestRate > 1 && loanModel.InterestRate <= 100)
            {
                loanModel.InterestRate /= 100m;
            }

            if (!ModelState.IsValid)
            {
                var userDTOs = await _accountServiceForWebApp.GetAllActiveUserByRole(Roles.Client.ToString());
                pageViewModel.Clients = (userDTOs != null) ? _mapper.Map<List<UserViewModel>>(userDTOs) : new List<UserViewModel>();
                return View(pageViewModel);
            }

            try
            {
                var loanDTO = _mapper.Map<CreateLoanDTO>(loanModel);
                await _loanService.AddLoanAsync(loanDTO, loanDTO.AssignedByAdminId, loanDTO.AdminFullName);
                TempData["SuccessMessage"] = "Préstamo asignado exitosamente.";
                // ⚠️ CAMBIO AQUÍ: Redirige al Index en caso de éxito
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                // ⚠️ CAMBIO AQUÍ: Usa TempData para mensajes de error que persistan en la redirección
                TempData["ErrorMessage"] = $"Error al asignar el préstamo: {ex.Message}";

                // No necesitas recargar los clientes aquí si vas a redirigir al Index.
                // Los datos de ModelState.AddModelError no persistirán después del RedirectToRoute.
                return RedirectToRoute(new { controller = "Loan", action = "Index" });
            }
        }

        public async Task<IActionResult> EditInterestRate(int id)
        {
            var loanDTO = await _loanService.GetLoanByIdAsync(id);
            if (loanDTO == null)
            {
                return NotFound();
            }
            var viewModel = _mapper.Map<EditLoanInterestRateViewModel>(loanDTO);
            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> EditInterestRate(EditLoanInterestRateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            try
            {
                decimal interestRateToUpdate = model.NewInterestRate;
                if (interestRateToUpdate > 1 && interestRateToUpdate <= 100)
                {
                    interestRateToUpdate /= 100m;
                }
                else if (interestRateToUpdate < 0 || interestRateToUpdate > 100)
                {
                    ModelState.AddModelError("NewInterestRate", "La tasa de interés debe estar entre 0 y 100.");
                    return View(model);
                }

                await _loanService.UpdateLoanInterestRateAsync(model.LoanId, interestRateToUpdate);
                TempData["SuccessMessage"] = "Tasa de interés del préstamo actualizada exitosamente.";
                return RedirectToAction(nameof(Details), new { id = model.LoanId });
            }
            catch (InvalidOperationException)
            {
                ModelState.AddModelError("", "Ocurrió un error de operación al actualizar la tasa de interés.");
                return View(model);
            }
            catch (Exception)
            {
                ModelState.AddModelError("", "Ocurrió un error inesperado al actualizar la tasa de interés.");
                return View(model);
            }
        }
    }
}