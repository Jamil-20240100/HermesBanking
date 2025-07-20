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

        private async Task<List<SelectListItem>> GetActiveSavingsAccounts(string? excludeAccountNumber = null)
        {
            // Obtener el clientId del cajero logueado
            var cashierId = _userManager.GetUserId(User);

            // Esperar la tarea y obtener las cuentas activas
            var accounts = await _cashierService.GetAllSavingsAccountsOfClients(cashierId);

            // Excluir la cuenta si es necesario
            if (!string.IsNullOrEmpty(excludeAccountNumber))
            {
                accounts = accounts.Where(a => a.AccountNumber != excludeAccountNumber).ToList();
            }

            // Mapear las cuentas a SelectListItem
            return accounts.Select(a => new SelectListItem
            {
                Value = a.AccountNumber,
                Text = $"{a.AccountNumber} - RD${a.Balance:N2}"
            }).ToList();
        }

        [HttpGet]
        public async Task<IActionResult> Deposit()
        {
            var vm = new DepositViewModel
            {
                SavingsAccount = await GetActiveSavingsAccounts()  // Añadido 'await' aquí
            };
            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> Deposit(DepositViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                vm.SavingsAccount = await GetActiveSavingsAccounts(); // Recarga lista si hay error
                return View(vm);
            }

            var cashierId = _userManager.GetUserId(User)!;
            var success = await _cashierService.MakeDepositAsync(vm.AccountNumber, vm.Amount, cashierId);

            if (!success)
            {
                ModelState.AddModelError("", "Cuenta no encontrada o inactiva.");
                vm.SavingsAccount = await GetActiveSavingsAccounts(); // Recarga lista si hay error
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
                vm.SavingsAccount = await GetActiveSavingsAccounts(); // Recarga lista si hay error
                return View("Deposit", vm);
            }

            var (account, fullName) = await _cashierService.GetAccountWithClientNameAsync(vm.AccountNumber);

            if (account == null || string.IsNullOrEmpty(fullName))
            {
                ModelState.AddModelError("", "La cuenta no existe o el usuario no está disponible.");
                vm.SavingsAccount = await GetActiveSavingsAccounts(); // Recarga lista si hay error
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
        public async Task<IActionResult> Withdraw()
        {
            var vm = new WithdrawViewModel
            {
                SavingsAccount = await GetActiveSavingsAccounts() // Añadido 'await' aquí
            };
            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> ConfirmWithdraw(WithdrawViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                vm.SavingsAccount = await GetActiveSavingsAccounts(); // Recarga lista si hay error
                return View("Withdraw", vm);
            }

            var (account, fullName) = await _cashierService.GetAccountWithClientNameAsync(vm.AccountNumber);

            if (account == null)
            {
                ModelState.AddModelError("", "La cuenta no existe o está inactiva.");
                vm.SavingsAccount = await GetActiveSavingsAccounts(); // Recarga lista si hay error
                return View("Withdraw", vm);
            }

            if (account.Balance < vm.Amount)
            {
                ModelState.AddModelError("", "La cuenta no tiene fondos suficientes.");
                vm.SavingsAccount = await GetActiveSavingsAccounts(); // Recarga lista si hay error
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
        public async Task<IActionResult> ThirdPartyTransfer()
        {
            // Esperar a obtener la lista de cuentas activas de forma asincrónica
            var accounts = await _cashierService.GetAllSavingsAccountsOfClients(User.Identity.Name);

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
                var accounts = await _cashierService.GetAllSavingsAccountsOfClients(User.Identity.Name);
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

            var confirmVm = new ConfirmThirdPartyTransferViewModel
            {
                SourceAccountNumber = sourceAccount.AccountNumber,
                DestinationAccountNumber = destAccount.AccountNumber,
                DestinationClientFullName = destName!,
                Amount = vm.Amount
            };

            return View("ConfirmThirdPartyTransfer", confirmVm);
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
        public async Task<IActionResult> CreditCardPayment()
        {
            var vm = new PagoTarjetaCreditoViewModel
            {
                SavingsAccount = await GetActiveSavingsAccounts() // Añadido 'await' aquí
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
                vm.SavingsAccount = await GetActiveSavingsAccounts(); // Recarga lista si hay error
                return View("CreditCardPayment", vm);  // Regresa a la vista original si hay error
            }

            var (cuenta, tarjeta, clienteNombre) = await _cashierService.GetAccountCardAndClientNameAsync(vm.AccountNumber, vm.CardNumber);

            if (cuenta == null || tarjeta == null)
            {
                ModelState.AddModelError("", "Cuenta o tarjeta no válida.");
                vm.SavingsAccount = await GetActiveSavingsAccounts(); // Recarga lista si hay error
                return View("CreditCardPayment", vm);  // Regresa a la vista original si hay error
            }

            if (cuenta.Balance < vm.Amount)
            {
                ModelState.AddModelError("", "Fondos insuficientes.");
                vm.SavingsAccount = await GetActiveSavingsAccounts(); // Recarga lista si hay error
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

            return View("ConfirmCreditCardPayment", confirmVm);  // Vista de confirmación después de validaciones
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
        public async Task<IActionResult> LoanPayment()
        {
            var vm = new PagoPrestamoViewModel
            {
                SavingsAccount = await GetActiveSavingsAccounts() // Añadido 'await' aquí
            };
            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> ConfirmLoanPayment(PagoPrestamoViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                vm.SavingsAccount = await GetActiveSavingsAccounts(); // Recarga lista si hay error
                return View("LoanPayment", vm);  // Devuelve a la misma vista con los datos correctos
            }

            var (account, _) = await _cashierService.GetAccountWithClientNameAsync(vm.AccountNumber);
            var (loan, clientName, debt) = await _cashierService.GetLoanInfoAsync(vm.LoanNumber);

            if (account == null || loan == null)
            {
                ModelState.AddModelError("", "Cuenta o préstamo no válido.");
                vm.SavingsAccount = await GetActiveSavingsAccounts(); // Recarga lista si hay error
                return View("LoanPayment", vm);  // Devuelve a la misma vista si hay error
            }

            if (account.Balance < vm.Amount)
            {
                ModelState.AddModelError("", "Fondos insuficientes.");
                vm.SavingsAccount = await GetActiveSavingsAccounts(); // Recarga lista si hay error
                return View("LoanPayment", vm);  // Devuelve a la misma vista si hay error
            }

            var confirmVm = new ConfirmPagoPrestamoViewModel
            {
                AccountNumber = account.AccountNumber,
                LoanIdentifier = loan.LoanIdentifier,
                ClientFullName = clientName,
                Amount = vm.Amount,
                DeudaActual = debt
            };

            TempData["Debug"] = "Entró a ConfirmLoanPayment";  // Esto es solo para depuración
            return View("ConfirmLoanPayment", confirmVm);
        }

        [HttpPost]
        public async Task<IActionResult> ExecuteLoanPayment(string loanIdentifier, string accountNumber, decimal amount)
        {
            var cashierId = _userManager.GetUserId(User)!;

            var (account, _) = await _cashierService.GetAccountWithClientNameAsync(accountNumber);
            var (loan, _, _) = await _cashierService.GetLoanInfoAsync(loanIdentifier);

            if (account == null || !account.IsActive)
            {
                TempData["Error"] = "La cuenta ingresada no es válida o está inactiva.";
                return RedirectToAction("LoanDetails", new { loanIdentifier });
            }

            if (loan == null || !loan.IsActive)
            {
                TempData["Error"] = "El préstamo no existe o ya fue completado.";
                return RedirectToAction("LoanDetails", new { loanIdentifier });
            }

            if (account.Balance < amount)
            {
                TempData["Error"] = "El monto excede el saldo disponible en la cuenta.";
                return RedirectToAction("LoanDetails", new { loanIdentifier });
            }

            var result = await _cashierService.MakeLoanPaymentAsync(loanIdentifier, accountNumber, amount, cashierId);

            if (!result)
            {
                TempData["Error"] = "No se pudo completar el pago. Intente de nuevo.";
                return RedirectToAction("LoanDetails", new { loanIdentifier });
            }

            TempData["Success"] = "Pago aplicado correctamente.";
            return RedirectToAction("Index", "CashierHome");
        }
    }
}
