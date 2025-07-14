using HermesBanking.Core.Application.Interfaces;
using HermesBanking.Core.Application.ViewModels.Cashier;
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

        public CashierController(ICashierService cashierService, UserManager<AppUser> userManager)
        {
            _cashierService = cashierService;
            _userManager = userManager;
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
                SavingsAccounts = GetActiveSavingsAccounts()
            };
            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> Deposit(DepositViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                vm.SavingsAccounts = GetActiveSavingsAccounts(); // Recarga lista si hay error
                return View(vm);
            }

            var cashierId = _userManager.GetUserId(User)!;
            var success = await _cashierService.MakeDepositAsync(vm.AccountNumber, vm.Amount, cashierId);


            if (!success)
            {
                ModelState.AddModelError("", "Cuenta no encontrada o inactiva.");
                vm.SavingsAccounts = GetActiveSavingsAccounts();
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
                vm.SavingsAccounts = GetActiveSavingsAccounts();
                return View("Deposit", vm);
            }

            var (account, fullName) = await _cashierService.GetAccountWithClientNameAsync(vm.AccountNumber);

            if (account == null || string.IsNullOrEmpty(fullName))
            {
                ModelState.AddModelError("", "La cuenta no existe o el usuario no está disponible.");
                vm.SavingsAccounts = GetActiveSavingsAccounts();
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
                SavingsAccounts = GetActiveSavingsAccounts()
            };
            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> ConfirmWithdraw(WithdrawViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                vm.SavingsAccounts = GetActiveSavingsAccounts();
                return View("Withdraw", vm);
            }

            var (account, fullName) = await _cashierService.GetAccountWithClientNameAsync(vm.AccountNumber);

            if (account == null)
            {
                ModelState.AddModelError("", "La cuenta no existe o está inactiva.");
                vm.SavingsAccounts = GetActiveSavingsAccounts();
                return View("Withdraw", vm);
            }

            if (account.Balance < vm.Amount)
            {
                ModelState.AddModelError("", "La cuenta no tiene fondos suficientes.");
                vm.SavingsAccounts = GetActiveSavingsAccounts();
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
                SavingsAccounts = GetActiveSavingsAccounts()
            };
            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> ConfirmCreditCardPayment(PagoTarjetaCreditoViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                vm.SavingsAccounts = GetActiveSavingsAccounts();
                return View("CreditCardPayment", vm);
            }

            var (cuenta, tarjeta, clienteNombre) = await _cashierService.GetAccountCardAndClientNameAsync(vm.AccountNumber, vm.CardNumber);

            if (cuenta == null || tarjeta == null)
            {
                ModelState.AddModelError("", "Cuenta o tarjeta no válida.");
                vm.SavingsAccounts = GetActiveSavingsAccounts();
                return View("CreditCardPayment", vm);
            }

            if (cuenta.Balance < vm.Amount)
            {
                ModelState.AddModelError("", "Fondos insuficientes.");
                vm.SavingsAccounts = GetActiveSavingsAccounts();
                return View("CreditCardPayment", vm);
            }

            var confirmVm = new ConfirmPagoTarjetaCreditoViewModel
            {
                AccountNumber = cuenta.AccountNumber,
                CardNumber = tarjeta.CardId,
                ClientFullName = clienteNombre,
                Amount = vm.Amount,
                DeudaActual = tarjeta.TotalOwedAmount,
            };

            return View("ConfirmCreditCardPayment", confirmVm);
        }

        [HttpPost]
        public async Task<IActionResult> ExecuteCreditCardPayment(string accountNumber, string cardNumber, decimal amount)
        {
            var cashierId = _userManager.GetUserId(User)!;
            var result = await _cashierService.MakeCreditCardPaymentAsync(accountNumber, cardNumber, amount, cashierId);

            if (!result)
            {
                TempData["Error"] = "Error al realizar el pago.";
                return RedirectToAction("CreditCardPayment");
            }

            TempData["Success"] = "Pago realizado correctamente.";
            return RedirectToAction("Index", "CashierHome");
        }

        [HttpGet]
        public IActionResult LoanPayment()
        {
            var vm = new PagoPrestamoViewModel
            {
                SavingsAccounts = GetActiveSavingsAccounts()
            };
            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> ConfirmLoanPayment(PagoPrestamoViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                vm.SavingsAccounts = GetActiveSavingsAccounts();
                return View("LoanPayment", vm);
            }

            var (cuenta, _) = await _cashierService.GetAccountWithClientNameAsync(vm.AccountNumber);
            var (prestamo, clienteNombre, deuda) = await _cashierService.GetLoanInfoAsync(vm.LoanNumber);

            if (cuenta == null || prestamo == null)
            {
                ModelState.AddModelError("", "Cuenta o préstamo no válido.");
                vm.SavingsAccounts = GetActiveSavingsAccounts();
                return View("LoanPayment", vm);
            }

            if (cuenta.Balance < vm.Amount)
            {
                ModelState.AddModelError("", "Fondos insuficientes.");
                vm.SavingsAccounts = GetActiveSavingsAccounts();
                return View("LoanPayment", vm);
            }

            var confirmVm = new ConfirmPagoPrestamoViewModel
            {
                AccountNumber = cuenta.AccountNumber,
                LoanNumber = prestamo.LoanNumber,
                ClientFullName = clienteNombre,
                Amount = vm.Amount,
                DeudaActual = deuda
            };

            return View("ConfirmLoanPayment", confirmVm);
        }

        [HttpPost]
        public async Task<IActionResult> ExecuteLoanPayment(string accountNumber, string loanNumber, decimal amount)
        {
            var cashierId = _userManager.GetUserId(User)!;
            var result = await _cashierService.MakeLoanPaymentAsync(accountNumber, loanNumber, amount, cashierId);

            if (!result)
            {
                TempData["Error"] = "Error al aplicar el pago.";
                return RedirectToAction("LoanPayment");
            }

            TempData["Success"] = "Pago aplicado correctamente.";
            return RedirectToAction("Index", "CashierHome");
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
