using AutoMapper;
using HermesBanking.Core.Application.DTOs;
using HermesBanking.Core.Application.DTOs.CreditCard;
using HermesBanking.Core.Application.DTOs.Loan;
using HermesBanking.Core.Application.DTOs.SavingsAccount;
using HermesBanking.Core.Application.DTOs.Transaction;
using HermesBanking.Core.Application.Interfaces;
using HermesBanking.Core.Application.ViewModels;
using HermesBanking.Core.Application.ViewModels.Beneficiary;
using HermesBanking.Core.Application.ViewModels.CreditCard;
using HermesBanking.Core.Application.ViewModels.Loan;
using HermesBanking.Core.Application.ViewModels.SavingsAccount;
using HermesBanking.Core.Application.ViewModels.Transaction;
using HermesBanking.Infrastructure.Identity.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace HermesBankingApp.Controllers
{
    [Authorize(Roles = "Client")]
    public class TransactionController : Controller
    {
        private readonly ISavingsAccountService _savingsAccountService;
        private readonly ILoanService _loanService;
        private readonly ICreditCardService _creditCardService;
        private readonly IBeneficiaryService _beneficiaryService;
        private readonly ITransactionService _transactionService;
        private readonly UserManager<AppUser> _userManager;
        private readonly IMapper _mapper;
        private readonly ILogger<TransactionController> _logger;

        public TransactionController(
            ISavingsAccountService savingsAccountService,
            ILoanService loanService,
            ICreditCardService creditCardService,
            IBeneficiaryService beneficiaryService,
            ITransactionService transactionService,
            UserManager<AppUser> userManager,
            IMapper mapper,
            ILogger<TransactionController> logger)
        {
            _savingsAccountService = savingsAccountService;
            _loanService = loanService;
            _creditCardService = creditCardService;
            _beneficiaryService = beneficiaryService;
            _transactionService = transactionService;
            _userManager = userManager;
            _mapper = mapper;
            _logger = logger;
        }

        // -------- INDEX --------
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Index", "Login");
            }

            var vm = await BuildTransactionViewModel(user.Id);
            return View(vm);
        }

        private async Task<ExpressTransactionViewModel> BuildTransactionViewModel(string clientId)
        {
            var allAccounts = await _savingsAccountService.GetAll();
            var clientAccounts = allAccounts
                .Where(a => a.ClientId == clientId && a.IsActive)
                .ToList();

            var vm = new ExpressTransactionViewModel
            {
                AvailableAccounts = _mapper.Map<List<SavingsAccountViewModel>>(clientAccounts)
            };
            return vm;
        }

        // -------- EXPRESS --------
        [HttpGet]
        public async Task<IActionResult> Express()
        {
            var user = await _userManager.GetUserAsync(User);
            var accounts = await _savingsAccountService.GetAll();
            var clientAccounts = accounts.Where(a => a.ClientId == user.Id && a.IsActive).ToList();

            var vm = new ExpressTransactionViewModel
            {
                AvailableAccounts = _mapper.Map<List<SavingsAccountViewModel>>(clientAccounts)
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Express(ExpressTransactionViewModel vm)
        {
            var user = await _userManager.GetUserAsync(User);
            var allClientAccounts = await _savingsAccountService.GetAll();
            var clientAccounts = allClientAccounts.Where(a => a.ClientId == user.Id && a.IsActive).ToList();

            // Validaciones
            if (!ModelState.IsValid)
            {
                vm.AvailableAccounts = _mapper.Map<List<SavingsAccountViewModel>>(clientAccounts);
                return View(vm);
            }

            // Validar cuentas
            var senderAccount = clientAccounts.FirstOrDefault(a => a.AccountNumber == vm.SenderAccountNumber);
            var receiverAccount = await _savingsAccountService.GetByAccountNumberAsync(vm.ReceiverAccountNumber);

            if (senderAccount == null || receiverAccount == null || !receiverAccount.IsActive || !senderAccount.IsActive)
            {
                ModelState.AddModelError("SenderAccountNumber", "Cuentas inválidas o inactivas.");
                vm.AvailableAccounts = _mapper.Map<List<SavingsAccountViewModel>>(clientAccounts);
                return View(vm);
            }

            // Validar fondos
            if (senderAccount.Balance < vm.Amount)
            {
                ModelState.AddModelError("Amount", "Fondos insuficientes en la cuenta origen.");
                vm.AvailableAccounts = _mapper.Map<List<SavingsAccountViewModel>>(clientAccounts);
                return View(vm);
            }

            // Crear DTO para la transacción
            var dto = new ExpressTransactionDTO
            {
                SenderAccountNumber = vm.SenderAccountNumber,
                ReceiverAccountNumber = vm.ReceiverAccountNumber,
                Amount = vm.Amount,
            };

            var wrapper = new ConfirmTransactionWrapperDTO
            {
                Type = "Express",
                JsonPayload = JsonConvert.SerializeObject(dto)
            };
            TempData["TransactionDetails"] = JsonConvert.SerializeObject(wrapper);

            // Redirigir a la página de confirmación
            return RedirectToAction(nameof(ConfirmTransactionExpress));
        }

        // -------- PAY CREDIT CARD --------
        [HttpGet]
        public async Task<IActionResult> PayCreditCard()
        {
            var user = await _userManager.GetUserAsync(User);

            // Obtener todas las cuentas y tarjetas de crédito disponibles para el cliente
            var accounts = await _savingsAccountService.GetAll();
            var creditCards = await _creditCardService.GetAll();

            // Filtrar las cuentas y tarjetas activas para este cliente
            var clientAccounts = accounts.Where(a => a.ClientId == user.Id && a.IsActive).ToList();
            var clientCards = creditCards.Where(c => c.ClientId == user.Id && c.IsActive).ToList();

            // Agregar depuración aquí para verificar los datos antes de la vista
            Console.WriteLine("Cuentas del cliente:");
            foreach (var account in clientAccounts)
            {
                Console.WriteLine($"Cuenta: {account.AccountNumber}, Balance: {account.Balance}");
            }

            Console.WriteLine("Tarjetas del cliente:");
            foreach (var card in clientCards)
            {
                Console.WriteLine($"Tarjeta: {card.CardId}, Activa: {card.IsActive}, Límite: {card.CreditLimit}");
            }

            // Mapear a ViewModel para la vista
            var vm = new PayCreditCardViewModel
            {
                AvailableAccounts = _mapper.Map<List<SavingsAccountViewModel>>(clientAccounts),
                AvailableCreditCards = _mapper.Map<List<CreditCardViewModel>>(clientCards)
            };

            return View(vm);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PayCreditCard(PayCreditCardViewModel vm)
        {
            var user = await _userManager.GetUserAsync(User);

            if (vm == null)
            {
                ModelState.AddModelError("", "El modelo no puede ser nulo.");
                return View(vm);
            }

            if (string.IsNullOrEmpty(vm.CreditCardId))
            {
                ModelState.AddModelError(nameof(vm.CreditCardId), "El ID de la tarjeta es obligatorio.");
                return View(vm);
            }

            if (string.IsNullOrEmpty(vm.FromAccountId))
            {
                ModelState.AddModelError(nameof(vm.FromAccountId), "La cuenta origen es obligatoria.");
                return View(vm);
            }

            try
            {
                // Obtener todas las cuentas activas asociadas al cliente (de la misma forma que en Express)
                var accounts = await _savingsAccountService.GetAll();
                var clientAccounts = accounts.Where(a => a.ClientId == user.Id && a.IsActive).ToList();

                // Obtener todas las tarjetas de crédito activas asociadas al cliente
                var creditCards = await _creditCardService.GetAll();
                var clientCards = creditCards.Where(c => c.ClientId == user.Id && c.IsActive).ToList();

                // Validar cuenta de origen usando la misma lógica de Express
                var senderAccount = clientAccounts.FirstOrDefault(a => a.AccountNumber == vm.FromAccountId);
                if (senderAccount == null)
                {
                    ModelState.AddModelError(nameof(vm.FromAccountId), "Cuenta origen no válida.");
                    vm.AvailableCreditCards = _mapper.Map<List<CreditCardViewModel>>(clientCards); // Mantener las tarjetas
                    return View(vm);
                }

                if (senderAccount.Balance < vm.Amount)
                {
                    ModelState.AddModelError(nameof(vm.Amount), "Fondos insuficientes en la cuenta origen.");
                    vm.AvailableCreditCards = _mapper.Map<List<CreditCardViewModel>>(clientCards); // Mantener las tarjetas
                    return View(vm);
                }

                // Validar tarjeta de crédito
                var creditCard = clientCards.FirstOrDefault(c => c.CardId == vm.CreditCardId);
                if (creditCard == null || !creditCard.IsActive)
                {
                    ModelState.AddModelError(nameof(vm.CreditCardId), "Tarjeta de crédito no válida o inactiva.");
                    vm.AvailableCreditCards = _mapper.Map<List<CreditCardViewModel>>(clientCards); // Mantener las tarjetas
                    return View(vm);
                }

                // Crear DTO para la transacción
                var creditDto = new PayCreditCardDTO
                {
                    FromAccountNumber = vm.FromAccountId,
                    CreditCardNumber = vm.CreditCardId,
                    Amount = vm.Amount,
                };

                // Guardar los detalles en TempData para la confirmación
                var wrapper = new ConfirmTransactionWrapperDTO
                {
                    Type = "CreditCard",
                    JsonPayload = JsonConvert.SerializeObject(creditDto)
                };

                TempData["TransactionDetails"] = JsonConvert.SerializeObject(wrapper);

                // Redirigir a la página de confirmación
                return RedirectToAction(nameof(ConfirmTransactionCreditCard));
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error en PayCreditCard: {ex.Message}");
                ModelState.AddModelError("", "Hubo un problema al procesar la solicitud.");
                return View(vm);
            }
        }











        // -------- PAY LOAN --------
        [HttpGet]
        public async Task<IActionResult> PayLoan()
        {
            var user = await _userManager.GetUserAsync(User);
            var accounts = await _savingsAccountService.GetAll();
            var loans = await _loanService.GetAllLoansAsync(null, null);

            var vm = new PayLoanViewModel
            {
                AvailableAccounts = _mapper.Map<List<SavingsAccountViewModel>>(
                    accounts.Where(a => a.ClientId == user.Id && a.IsActive).ToList()),
                AvailableLoans = _mapper.Map<List<LoanViewModel>>(
                    loans.Where(l => l.ClientId == user.Id && l.IsActive).ToList())
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PayLoan(PayLoanViewModel vm)
        {
            var user = await _userManager.GetUserAsync(User);

            if (!ModelState.IsValid)
                return View(vm);

            var hasFunds = await _transactionService.HasSufficientFunds(vm.SavingsAccountNumber, vm.Amount);
            if (!hasFunds)
            {
                ModelState.AddModelError(nameof(vm.Amount), "Fondos insuficientes.");
                return View(vm);
            }

            var loanDto = new PayLoanDTO
            {
                FromAccountNumber = vm.SavingsAccountNumber,
                LoanCode = vm.LoanId,
                Amount = vm.Amount,
            };

            var wrapper = new ConfirmTransactionWrapperDTO
            {
                Type = "Loan",
                JsonPayload = JsonConvert.SerializeObject(loanDto)
            };
            TempData["TransactionDetails"] = JsonConvert.SerializeObject(wrapper);

            return RedirectToAction(nameof(ConfirmTransactionLoan));
        }

        // -------- PAY BENEFICIARY --------
        [HttpGet]
        public async Task<IActionResult> PayBeneficiary()
        {
            var user = await _userManager.GetUserAsync(User);
            var accounts = await _savingsAccountService.GetAll();
            var beneficiaries = await _beneficiaryService.GetAll();

            var vm = new TransferToBeneficiaryViewModel
            {
                AvailableAccounts = _mapper.Map<List<SavingsAccountViewModel>>(accounts.Where(a => a.ClientId == user.Id && a.IsActive).ToList()),
                AvailableBeneficiaries = _mapper.Map<List<BeneficiaryViewModel>>(
                    beneficiaries.Where(b => b.ClientId == user.Id).ToList())
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PayBeneficiary(TransferToBeneficiaryViewModel vm)
        {
            var user = await _userManager.GetUserAsync(User);

            if (!ModelState.IsValid)
                return View(vm);

            var hasFunds = await _transactionService.HasSufficientFunds(vm.FromAccountId, vm.Amount);
            if (!hasFunds)
            {
                ModelState.AddModelError(nameof(vm.Amount), "Fondos insuficientes.");
                return View(vm);
            }

            var beneficiaryDto = new PayBeneficiaryDTO
            {
                FromAccountNumber = vm.FromAccountId,
                BeneficiaryAccountNumber = vm.BeneficiaryId,
                Amount = vm.Amount,
            };

            var wrapper = new ConfirmTransactionWrapperDTO
            {
                Type = "Beneficiary",
                JsonPayload = JsonConvert.SerializeObject(beneficiaryDto)
            };
            TempData["TransactionDetails"] = JsonConvert.SerializeObject(wrapper);

            return RedirectToAction(nameof(ConfirmTransactionBeneficiary));
        }

        // -------- CONFIRM TRANSACTION EXPRESS --------
        [HttpGet]
        public IActionResult ConfirmTransactionExpress()
        {
            var wrapperJson = TempData["TransactionDetails"] as string;

            if (string.IsNullOrEmpty(wrapperJson))
                return RedirectToAction(nameof(Index));

            var wrapper = JsonConvert.DeserializeObject<ConfirmTransactionWrapperDTO>(wrapperJson);
            var expressDto = JsonConvert.DeserializeObject<ExpressTransactionDTO>(wrapper.JsonPayload);

            var vm = new ExpressTransactionViewModel
            {
                SenderAccountNumber = expressDto.SenderAccountNumber,
                ReceiverAccountNumber = expressDto.ReceiverAccountNumber,
                Amount = expressDto.Amount
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmTransactionExpress(ExpressTransactionViewModel vm, string confirm)
        {
            if (confirm != "yes")
            {
                _logger.LogWarning("Confirmación no válida. Redirigiendo.");
                return RedirectToAction(nameof(Index)); // Redirige si no se confirma la transacción
            }

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("El estado del modelo no es válido.");
                return View(vm); // Si el modelo no es válido, muestra la vista con los errores
            }

            // Ejecutar la transacción
            var dto = new ExpressTransactionDTO
            {
                SenderAccountNumber = vm.SenderAccountNumber,
                ReceiverAccountNumber = vm.ReceiverAccountNumber,
                Amount = vm.Amount
            };

            try
            {
                await _transactionService.ExecuteExpressTransactionAsync(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error ejecutando la transacción: {ex.Message}");
                ModelState.AddModelError("", "Hubo un problema al ejecutar la transacción.");
                return View(vm); // Si hay un error, muestra el mensaje
            }

            return RedirectToAction("Index", "ClientHome"); // Redirige a la página de inicio si la transacción fue exitosa
        }


        // -------- CONFIRM TRANSACTION LOAN --------
        [HttpGet]
        public IActionResult ConfirmTransactionLoan()
        {
            var wrapperJson = TempData["TransactionDetails"] as string;

            if (string.IsNullOrEmpty(wrapperJson))
                return RedirectToAction(nameof(Index));

            var wrapper = JsonConvert.DeserializeObject<ConfirmTransactionWrapperDTO>(wrapperJson);
            var loanDto = JsonConvert.DeserializeObject<PayLoanDTO>(wrapper.JsonPayload);

            var vm = new PayLoanViewModel
            {
                SavingsAccountNumber = loanDto.FromAccountNumber,
                LoanId = loanDto.LoanCode,
                Amount = loanDto.Amount
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmTransactionLoan(PayLoanViewModel vm, string confirm)
        {
            if (confirm != "yes")
            {
                _logger.LogWarning("Confirmación no válida. Redirigiendo.");
                return RedirectToAction(nameof(Index)); // Redirige si no se confirma la transacción
            }

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("El estado del modelo no es válido.");
                return View(vm); // Si el modelo no es válido, muestra la vista con los errores
            }

            // Ejecutar la transacción
            var dto = new PayLoanDTO
            {
                FromAccountNumber = vm.SavingsAccountNumber,
                LoanCode = vm.LoanId,
                Amount = vm.Amount
            };

            try
            {
                await _transactionService.ExecutePayLoanTransactionAsync(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error ejecutando la transacción: {ex.Message}");
                ModelState.AddModelError("", "Hubo un problema al ejecutar la transacción.");
                return View(vm); // Si hay un error, muestra el mensaje
            }

            return RedirectToAction("Index", "ClientHome"); // Redirige a la página de inicio si la transacción fue exitosa
        }


        // -------- CONFIRM TRANSACTION BENEFICIARY --------
        [HttpGet]
        public IActionResult ConfirmTransactionBeneficiary()
        {
            var wrapperJson = TempData["TransactionDetails"] as string;

            if (string.IsNullOrEmpty(wrapperJson))
                return RedirectToAction(nameof(Index));

            var wrapper = JsonConvert.DeserializeObject<ConfirmTransactionWrapperDTO>(wrapperJson);
            var beneficiaryDto = JsonConvert.DeserializeObject<PayBeneficiaryDTO>(wrapper.JsonPayload);

            var vm = new TransferToBeneficiaryViewModel
            {
                FromAccountId = beneficiaryDto.FromAccountNumber,
                BeneficiaryId = beneficiaryDto.BeneficiaryAccountNumber,
                Amount = beneficiaryDto.Amount
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmTransactionBeneficiary(ConfirmTransactionBeneficiaryViewModel vm, string confirm)
        {
            if (confirm != "yes")
            {
                _logger.LogWarning("Confirmación no válida. Redirigiendo.");
                return RedirectToAction(nameof(Index)); // Redirige si no se confirma la transacción
            }

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("El estado del modelo no es válido.");
                return View(vm); // Si el modelo no es válido, muestra la vista con los errores
            }

            // Ejecutar la transacción
            var dto = new PayBeneficiaryDTO
            {
                FromAccountNumber = vm.FromAccountId,
                BeneficiaryAccountNumber = vm.ClientId,
                Amount = vm.Amount
            };

            try
            {
                await _transactionService.ExecutePayBeneficiaryTransactionAsync(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error ejecutando la transacción: {ex.Message}");
                ModelState.AddModelError("", "Hubo un problema al ejecutar la transacción.");
                return View(vm); // Si hay un error, muestra el mensaje
            }

            return RedirectToAction("Index", "ClientHome"); // Redirige a la página de inicio si la transacción fue exitosa
        }


        // -------- CONFIRM TRANSACTION CREDIT CARD --------
        [HttpGet]
        public IActionResult ConfirmTransactionCreditCard()
        {
            var wrapperJson = TempData["TransactionDetails"] as string;

            if (string.IsNullOrEmpty(wrapperJson))
                return RedirectToAction(nameof(Index));

            var wrapper = JsonConvert.DeserializeObject<ConfirmTransactionWrapperDTO>(wrapperJson);
            var creditCardDto = JsonConvert.DeserializeObject<PayCreditCardDTO>(wrapper.JsonPayload);

            var vm = new ConfirmTransactionCreditCardViewModel
            {
                FromAccountId = creditCardDto.FromAccountNumber,
                CreditCardId = creditCardDto.CreditCardNumber,
                Amount = creditCardDto.Amount
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmTransactionCreditCard(ConfirmTransactionCreditCardViewModel vm, string confirm)
        {
            if (confirm != "yes")
            {
                _logger.LogWarning("Confirmación no válida. Redirigiendo.");
                return RedirectToAction(nameof(Index)); // Redirige si no se confirma la transacción
            }

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("El estado del modelo no es válido.");
                return View(vm); // Si el modelo no es válido, muestra la vista con los errores
            }

            // Crear DTO con los detalles del pago
            var dto = new PayCreditCardDTO
            {
                FromAccountNumber = vm.FromAccountId,
                CreditCardNumber = vm.CreditCardId,
                Amount = vm.Amount
            };

            if (dto.Amount <= 0)
            {
                ModelState.AddModelError("", "El monto debe ser mayor a cero.");
                return View(vm);
            }

            try
            {
                // Realizar la transacción de pago de tarjeta
                await _transactionService.ExecutePayCreditCardTransactionAsync(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error ejecutando la transacción: {ex.Message}");
                ModelState.AddModelError("", "Hubo un problema al ejecutar la transacción.");
                return View(vm); // Si hay un error, muestra el mensaje
            }

            // Redirigir a la página de inicio después de una transacción exitosa
            TempData["Success"] = "Pago realizado correctamente.";
            return RedirectToAction("Index", "ClientHome");
        }


    }
}