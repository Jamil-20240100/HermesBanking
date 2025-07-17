using AutoMapper;
using HermesBanking.Core.Application.DTOs.Cashier;
using HermesBanking.Core.Application.Interfaces;
using HermesBanking.Core.Application.ViewModels.Cashier;
using HermesBanking.Core.Application.ViewModels.Loan;
using HermesBanking.Infrastructure.Identity.Entities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HermesBankingApp.Controllers
{
    [Authorize(Roles = "Cashier")]
    public class CashierController : Controller
    {
        private readonly ICashierService _cashierService;
        private readonly UserManager<AppUser> _userManager;
        private readonly IMapper _mapper;

        public CashierController(ICashierService cashierService, UserManager<AppUser> userManager, IMapper mapper)
        {
            _cashierService = cashierService;
            _userManager = userManager;
            _mapper = mapper;
        }
        
        private List<SelectListItem> GetActiveSavingsAccounts(string? excludeAccountNumber = null)
        {
            var accounts = _cashierService.GetAllActiveAccounts();

            if (!string.IsNullOrEmpty(excludeAccountNumber))
            {
                accounts = accounts.Where(a => a.AccountNumber != excludeAccountNumber).ToList();
            }

            return accounts.Select(a => new SelectListItem
            {
                Value = a.AccountNumber,
                Text = $"{a.AccountNumber} - RD${a.Balance:N2}"
            }).ToList();
        }

        [HttpGet]
        public IActionResult Deposit()
        {
            var vm = new DepositViewModel
            {
                SavingsAccount = GetActiveSavingsAccounts()
            };
            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> Deposit(DepositViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                vm.SavingsAccount = GetActiveSavingsAccounts(); // Recarga lista si hay error
                return View(vm);
            }

            var cashierId = _userManager.GetUserId(User)!;
            var success = await _cashierService.MakeDepositAsync(vm.AccountNumber, vm.Amount, cashierId);


            if (!success)
            {
                ModelState.AddModelError("", "Cuenta no encontrada o inactiva.");
                vm.SavingsAccount = GetActiveSavingsAccounts();
                return View(vm);
            }

            TempData["Success"] = "Depósito realizado exitosamente.";
            return RedirectToAction("Deposit");
        }

        [HttpPost]
        public async Task<IActionResult> ConfirmDeposit(DepositViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                vm.SavingsAccount = GetActiveSavingsAccounts();
                return View("Deposit", vm);
            }

            var (account, fullName) = await _cashierService.GetAccountWithClientNameAsync(vm.AccountNumber);

            if (account == null || string.IsNullOrEmpty(fullName))
            {
                ModelState.AddModelError("", "La cuenta no existe o el usuario no está disponible.");
                vm.SavingsAccount = GetActiveSavingsAccounts();
                return View("Deposit", vm);
            }

            var confirmVm = new ConfirmDepositViewModel
            {
                AccountNumber = account.AccountNumber,
                ClientFullName = fullName,
                Amount = vm.Amount
            };

            return View("ConfirmDeposit", confirmVm);
        }

        [HttpPost]
        public async Task<IActionResult> ExecuteDeposit(string accountNumber, decimal amount)
        {
            var cashierId = _userManager.GetUserId(User)!;
            var success = await _cashierService.MakeDepositAsync(accountNumber, amount, cashierId);


            if (!success)
            {
                TempData["Error"] = "Error al realizar el depósito.";
                return RedirectToAction("Deposit");
            }

            TempData["Success"] = "Depósito realizado exitosamente.";
            return RedirectToAction("Index", "CashierHome");
        }

        [HttpGet]
        public IActionResult Withdraw()
        {
            var vm = new WithdrawViewModel
            {
                SavingsAccount = GetActiveSavingsAccounts()
            };
            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> ConfirmWithdraw(WithdrawViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                vm.SavingsAccount = GetActiveSavingsAccounts();
                return View("Withdraw", vm);
            }

            var (account, fullName) = await _cashierService.GetAccountWithClientNameAsync(vm.AccountNumber);

            if (account == null)
            {
                ModelState.AddModelError("", "La cuenta no existe o está inactiva.");
                vm.SavingsAccount = GetActiveSavingsAccounts();
                return View("Withdraw", vm);
            }

            if (account.Balance < vm.Amount)
            {
                ModelState.AddModelError("", "La cuenta no tiene fondos suficientes.");
                vm.SavingsAccount = GetActiveSavingsAccounts();
                return View("Withdraw", vm);
            }

            var confirmVm = new ConfirmWithdrawViewModel
            {
                AccountNumber = account.AccountNumber,
                ClientFullName = fullName!,
                Amount = vm.Amount
            };

            return View("ConfirmWithdraw", confirmVm);
        }

        [HttpPost]
        public async Task<IActionResult> ExecuteWithdraw(string accountNumber, decimal amount)
        {
            var cashierId = _userManager.GetUserId(User)!;
            var result = await _cashierService.MakeWithdrawAsync(accountNumber, amount, cashierId);


            if (!result)
            {
                TempData["Error"] = "No se pudo completar el retiro.";
                return RedirectToAction("Withdraw");
            }

            TempData["Success"] = "Retiro realizado correctamente.";
            return RedirectToAction("Index", "CashierHome");
        }

        [HttpGet]
        public IActionResult ThirdPartyTransfer()
        {
            var accounts = _cashierService.GetAllActiveAccounts();

            var items = accounts.Select(a => new SelectListItem
            {
                Value = a.AccountNumber,
                Text = $"{a.AccountNumber} - RD$ {a.Balance:N2}"
            }).ToList();

            var viewModel = new ThirdPartyTransferViewModel
            {
                SourceAccounts = items,
                DestinationAccounts = items // mismos elementos
            };

            return View(viewModel);
        }


        [HttpPost]
        public async Task<IActionResult> ConfirmThirdPartyTransfer(ThirdPartyTransferViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                // Recarga las listas por si vuelves a la vista original
                var accounts = _cashierService.GetAllActiveAccounts();
                var items = accounts.Select(a => new SelectListItem
                {
                    Value = a.AccountNumber,
                    Text = $"{a.AccountNumber} - RD$ {a.Balance:N2}"
                }).ToList();

                vm.SourceAccounts = items;
                vm.DestinationAccounts = items;
                return View("ThirdPartyTransfer", vm);
            }

            var (sourceAccount, _) = await _cashierService.GetAccountWithClientNameAsync(vm.SourceAccountNumber);
            var (destAccount, destName) = await _cashierService.GetAccountWithClientNameAsync(vm.DestinationAccountNumber);

            if (sourceAccount == null || destAccount == null)
            {
                TempData["Error"] = "Cuentas inválidas.";
                return RedirectToAction("ThirdPartyTransfer");
            }

            if (sourceAccount.Balance < vm.Amount)
            {
                TempData["Error"] = "Fondos insuficientes.";
                return RedirectToAction("ThirdPartyTransfer");
            }

            // ✅ Aquí estás pasando el ViewModel correcto a la vista de confirmación
            var confirmVm = new ConfirmThirdPartyTransferViewModel
            {
                SourceAccountNumber = sourceAccount.AccountNumber,
                DestinationAccountNumber = destAccount.AccountNumber,
                DestinationClientFullName = destName!,
                Amount = vm.Amount
            };

            return View("ConfirmThirdPartyTransfer", confirmVm); // <-- El tipo correcto
        }

        [HttpPost]
        public async Task<IActionResult> ExecuteThirdPartyTransfer(string sourceAccountNumber, string destinationAccountNumber, decimal amount)
        {
            var cashierId = _userManager.GetUserId(User)!;
            var success = await _cashierService.MakeThirdPartyTransferAsync(sourceAccountNumber, destinationAccountNumber, amount, cashierId);


            if (!success)
            {
                TempData["Error"] = "Error al realizar la transferencia.";
                return RedirectToAction("ThirdPartyTransfer");
            }

            TempData["Success"] = "Transferencia completada.";
            return RedirectToAction("Index", "CashierHome");
        }

        [HttpGet]
        public IActionResult CreditCardPayment()
        {
            var vm = new PagoTarjetaCreditoViewModel
            {
                SavingsAccount = GetActiveSavingsAccounts()
            };
            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> PayCreditCard(CreditCardPaymentDto paymentDto)
        {
            try
            {
                // Obtener el ID del cajero
                var cashierId = _userManager.GetUserId(User);

                // Procesar el pago de la tarjeta con los parámetros correctos
                var result = await _cashierService.MakeCreditCardPaymentAsync(
                    paymentDto.AccountNumber,
                    paymentDto.CardNumber,
                    paymentDto.Amount,
                    cashierId);

                if (result)
                {
                    TempData["Success"] = "El pago se ha realizado correctamente.";
                }
                else
                {
                    TempData["Error"] = "Hubo un error al procesar el pago.";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error: {ex.Message}";
            }

            // Redirigir de nuevo a la vista principal del Cajero
            return RedirectToAction("Index", "CashierHome");
        }


        [HttpPost]
        public async Task<IActionResult> ConfirmCreditCardPayment(PagoTarjetaCreditoViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                vm.SavingsAccount = GetActiveSavingsAccounts();
                return View("CreditCardPayment", vm);  // Regresa a la vista original si hay error
            }

            var (cuenta, tarjeta, clienteNombre) = await _cashierService.GetAccountCardAndClientNameAsync(vm.AccountNumber, vm.CardNumber);

            if (cuenta == null || tarjeta == null)
            {
                ModelState.AddModelError("", "Cuenta o tarjeta no válida.");
                vm.SavingsAccount = GetActiveSavingsAccounts();
                return View("CreditCardPayment", vm);  // Regresa a la vista original si hay error
            }

            if (cuenta.Balance < vm.Amount)
            {
                ModelState.AddModelError("", "Fondos insuficientes.");
                vm.SavingsAccount = GetActiveSavingsAccounts();
                return View("CreditCardPayment", vm);  // Regresa a la vista original si hay error
            }

            var confirmVm = new ConfirmPagoTarjetaCreditoViewModel
            {
                AccountNumber = cuenta.AccountNumber,
                CardNumber = tarjeta.CardId,
                ClientFullName = clienteNombre,
                Amount = vm.Amount,
                DeudaActual = tarjeta.TotalOwedAmount,
            };

            return View("ConfirmCreditCardPayment", confirmVm);  // Esta es la vista a la que deberías ir después de la validación
        }


        [HttpPost]
        public async Task<IActionResult> ExecuteCreditCardPayment(string accountNumber, string cardNumber, decimal amount)
        {
            // Validación de parámetros
            if (string.IsNullOrEmpty(accountNumber) || string.IsNullOrEmpty(cardNumber) || amount <= 0)
            {
                TempData["Error"] = "Datos inválidos para procesar el pago.";
                return RedirectToAction("CreditCardPayment");
            }

            var cashierId = _userManager.GetUserId(User);
            if (cashierId == null)
            {
                TempData["Error"] = "Usuario no autenticado.";
                return RedirectToAction("Login", "Account");
            }

            // Realiza la operación del pago
            var result = await _cashierService.MakeCreditCardPaymentAsync(accountNumber, cardNumber, amount, cashierId);

            // Verifica si la operación fue exitosa
            if (!result)
            {
                TempData["Error"] = "Error al realizar el pago.";
                return RedirectToAction("CreditCardPayment");
            }

            // Redirección después de pago exitoso
            TempData["Success"] = "Pago realizado correctamente.";
            return RedirectToAction("Index", "CashierHome");
        }


        [HttpGet]
        public IActionResult LoanPayment()
        {
            var vm = new PagoPrestamoViewModel
            {
                SavingsAccount = GetActiveSavingsAccounts()
            };
            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> ConfirmLoanPayment(PagoPrestamoViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                vm.SavingsAccount = GetActiveSavingsAccounts();
                return View("LoanPayment", vm);
            }

            var (cuenta, _) = await _cashierService.GetAccountWithClientNameAsync(vm.AccountNumber);
            var (prestamo, clienteNombre, deuda) = await _cashierService.GetLoanInfoAsync(vm.LoanNumber);

            if (cuenta == null || prestamo == null)
            {
                ModelState.AddModelError("", "Cuenta o préstamo no válido.");
                vm.SavingsAccount = GetActiveSavingsAccounts();
                return View("LoanPayment", vm);
            }

            if (cuenta.Balance < vm.Amount)
            {
                ModelState.AddModelError("", "Fondos insuficientes.");
                vm.SavingsAccount = GetActiveSavingsAccounts();
                return View("LoanPayment", vm);
            }

            var confirmVm = new ConfirmPagoPrestamoViewModel
            {
                AccountNumber = cuenta.AccountNumber,
                LoanIdentifier = prestamo.LoanIdentifier,
                ClientFullName = clienteNombre,
                Amount = vm.Amount,
                DeudaActual = deuda
            };

            TempData["Debug"] = "Entró a ConfirmLoanPayment";
            return View("ConfirmLoanPayment", confirmVm);
        }

        [HttpPost]
        public async Task<IActionResult> ExecuteLoanPayment(string loanIdentifier)
        {
            var cashierId = _userManager.GetUserId(User)!;
            var result = await _cashierService.MakeLoanPaymentAsync(loanIdentifier, cashierId);

            if (!result)
            {
                TempData["Error"] = "Error al aplicar el pago.";
                return RedirectToAction("Details", "Loan", new { loanIdentifier });
            }

            TempData["Success"] = "Pago aplicado correctamente.";
            return RedirectToAction("Index", "CashierHome");
        }


        [HttpPost]
        public async Task<IActionResult> PayLoan(string loanIdentifier)
        {
            var cashierId = _userManager.GetUserId(User)!;

            var result = await _cashierService.MakeLoanPaymentAsync(loanIdentifier, cashierId);

            if (!result)
            {
                TempData["Error"] = "No se pudo completar el pago. Verifique si ya está saldado.";
                return RedirectToAction("Details", "Loan", new { loanIdentifier });
            }

            TempData["Success"] = "Pago aplicado correctamente.";
            return RedirectToAction("Index", "CashierHome");
        }

        [HttpGet]
        public async Task<IActionResult> LoanDetails(string loanIdentifier)
        {
            var (loan, clientFullName, remainingDebt) = await _cashierService.GetLoanInfoAsync(loanIdentifier);

            if (loan == null)
            {
                TempData["Error"] = "Préstamo no encontrado o inactivo.";
                return RedirectToAction("Index", "CashierHome");
            }

            var vm = new LoanDetailsViewModel
            {
                Id = loan.Id,
                LoanIdentifier = loan.LoanIdentifier,
                ClientId = loan.ClientId,
                Amount = loan.Amount,
                InterestRate = loan.InterestRate,
                LoanTermMonths = loan.LoanTermMonths,
                MonthlyInstallmentValue = loan.MonthlyInstallmentValue,
                TotalInstallments = loan.TotalInstallments,
                PaidInstallments = loan.PaidInstallments,
                PendingAmount = loan.PendingAmount,
                IsActive = loan.IsActive,
                IsOverdue = loan.IsOverdue,
                AssignedByAdminId = loan.AssignedByAdminId,
                AdminFullName = loan.AdminFullName,
                CreatedAt = loan.CreatedAt,
                CompletedAt = loan.CompletedAt,
                AmortizationSchedule = loan.AmortizationInstallments?.Select(a => _mapper.Map<AmortizationInstallmentViewModel>(a)).ToList() ?? new List<AmortizationInstallmentViewModel>(),
                ClientFullName = clientFullName,
                RemainingDebt = remainingDebt
            };

            return View(vm);
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(IdentityConstants.ApplicationScheme);
            return RedirectToAction("Index", "Login");
        }
        
    }
}
