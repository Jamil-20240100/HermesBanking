using HermesBanking.Core.Application.DTOs.Transaction;
using HermesBanking.Core.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering; // Para SelectList
using System.Security.Claims; // Para obtener el ID del usuario

namespace HermesBankingApp.Controllers
{
    // Considera usar [Authorize] aquí para proteger todas las acciones del controlador
    // [Authorize]
    public class TransactionController : Controller
    {
        private readonly ITransactionService _transactionService;
        private readonly IAccountServiceForWebApp _accountServiceForWebApp; // Para obtener cuentas/tarjetas/préstamos del usuario
        private readonly IBeneficiaryService _beneficiaryService; // Para obtener beneficiarios del usuario
        private readonly ILogger<TransactionController> _logger;

        public TransactionController(
            ITransactionService transactionService,
            IAccountServiceForWebApp accountServiceForWebApp,
            IBeneficiaryService beneficiaryService,
            ILogger<TransactionController> logger)
        {
            _transactionService = transactionService;
            _accountServiceForWebApp = accountServiceForWebApp;
            _beneficiaryService = beneficiaryService;
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        private string GetCurrentUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        }

        #region Transferencias Entre Cuentas
        // GET: /Transaction/Transfer
        public async Task<IActionResult> Transfer()
        {
            var userId = GetCurrentUserId();
            var accounts = await _accountServiceForWebApp.GetSavingsAccountsByUserIdAsync(userId); // Asume que este método existe y retorna List<SavingsAccountDTO>

            var model = new TransactionRequestDto
            {
                AvailableAccounts = accounts.Where(a => a.IsActive).ToList() // Filtra solo cuentas activas
            };
            ViewBag.SourceAccounts = new SelectList(model.AvailableAccounts, "AccountNumber", "DisplayText");
            ViewBag.DestinationAccounts = new SelectList(model.AvailableAccounts, "AccountNumber", "DisplayText");
            return View(model);
        }

        // POST: /Transaction/Transfer
        [HttpPost]
        [ValidateAntiForgeryToken] // Protección CSRF
        public async Task<IActionResult> Transfer(TransactionRequestDto request)
        {
            var userId = GetCurrentUserId();
            var accounts = await _accountServiceForWebApp.GetSavingsAccountsByUserIdAsync(userId);
            request.AvailableAccounts = accounts.Where(a => a.IsActive).ToList(); // Vuelve a cargar para re-popular el dropdown si hay errores

            if (!ModelState.IsValid)
            {
                ViewBag.SourceAccounts = new SelectList(request.AvailableAccounts, "AccountNumber", "DisplayText", request.SourceAccountNumber);
                ViewBag.DestinationAccounts = new SelectList(request.AvailableAccounts, "AccountNumber", "DisplayText", request.DestinationAccountNumber);
                TempData["ErrorMessage"] = "Por favor, corrige los errores del formulario.";
                return View(request);
            }

            try
            {
                await _transactionService.PerformTransactionAsync(request); // Llama a tu servicio de dominio
                TempData["SuccessMessage"] = "Transferencia realizada exitosamente.";
                _logger.LogInformation("Transferencia exitosa de {SourceAccount} a {DestinationAccount} por {Amount:C} para el usuario {UserId}.",
                    request.SourceAccountNumber, request.DestinationAccountNumber, request.Amount, userId);
                return RedirectToAction("TransferConfirmation"); // Redirige a una página de confirmación
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Transferencia fallida (validación) de {SourceAccount} a {DestinationAccount} para el usuario {UserId}: {ErrorMessage}",
                    request.SourceAccountNumber, request.DestinationAccountNumber, userId, ex.Message);
                TempData["ErrorMessage"] = ex.Message; // Muestra el mensaje de validación al usuario
                ViewBag.SourceAccounts = new SelectList(request.AvailableAccounts, "AccountNumber", "DisplayText", request.SourceAccountNumber);
                ViewBag.DestinationAccounts = new SelectList(request.AvailableAccounts, "AccountNumber", "DisplayText", request.DestinationAccountNumber);
                return View(request);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al procesar la transferencia de {SourceAccount} a {DestinationAccount} para el usuario {UserId}.",
                    request.SourceAccountNumber, request.DestinationAccountNumber, userId);
                TempData["ErrorMessage"] = "Ocurrió un error inesperado al procesar la transferencia. Inténtalo de nuevo.";
                ViewBag.SourceAccounts = new SelectList(request.AvailableAccounts, "AccountNumber", "DisplayText", request.SourceAccountNumber);
                ViewBag.DestinationAccounts = new SelectList(request.AvailableAccounts, "AccountNumber", "DisplayText", request.DestinationAccountNumber);
                return View(request);
            }
        }

        // GET: /Transaction/TransferConfirmation
        public IActionResult TransferConfirmation()
        {
            return View();
        }
        #endregion

        #region Pagos de Tarjeta de Crédito
        // GET: /Transaction/PayCreditCard
        public async Task<IActionResult> PayCreditCard()
        {
            var userId = GetCurrentUserId();
            var accounts = await _accountServiceForWebApp.GetSavingsAccountsByUserIdAsync(userId);
            var creditCards = await _accountServiceForWebApp.GetCreditCardsByUserIdAsync(userId); // Asume que este método existe y retorna List<CreditCardDTO>

            var model = new CreditCardPaymentDto
            {
                AvailableAccounts = accounts.Where(a => a.IsActive).ToList(),
                AvailableCreditCards = creditCards.Where(cc => cc.IsActive).ToList() // Filtra solo tarjetas activas
            };
            ViewBag.SourceAccounts = new SelectList(model.AvailableAccounts, "AccountNumber", "DisplayText");
            ViewBag.CreditCards = new SelectList(model.AvailableCreditCards, "CardId", "DisplayText"); // Usa CardId como valor
            return View(model);
        }

        // POST: /Transaction/PayCreditCard
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PayCreditCard(CreditCardPaymentDto paymentDto)
        {
            var userId = GetCurrentUserId();
            var accounts = await _accountServiceForWebApp.GetSavingsAccountsByUserIdAsync(userId);
            var creditCards = await _accountServiceForWebApp.GetCreditCardsByUserIdAsync(userId);
            paymentDto.AvailableAccounts = accounts.Where(a => a.IsActive).ToList();
            paymentDto.AvailableCreditCards = creditCards.Where(cc => cc.IsActive).ToList();

            if (!ModelState.IsValid)
            {
                ViewBag.SourceAccounts = new SelectList(paymentDto.AvailableAccounts, "AccountNumber", "DisplayText", paymentDto.SourceAccountNumber);
                ViewBag.CreditCards = new SelectList(paymentDto.AvailableCreditCards, "CardId", "DisplayText", paymentDto.CreditCardNumber);
                TempData["ErrorMessage"] = "Por favor, corrige los errores del formulario.";
                return View(paymentDto);
            }

            try
            {
                await _transactionService.PayCreditCardAsync(paymentDto);
                TempData["SuccessMessage"] = "Pago de tarjeta de crédito realizado exitosamente.";
                _logger.LogInformation("Pago de tarjeta de crédito exitoso desde {SourceAccount} para tarjeta {CreditCardNumber} por {Amount:C} para el usuario {UserId}.",
                    paymentDto.SourceAccountNumber, paymentDto.CreditCardNumber, paymentDto.Amount, userId);
                return RedirectToAction("CreditCardPaymentConfirmation");
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Pago de tarjeta de crédito fallido (validación) desde {SourceAccount} para tarjeta {CreditCardNumber} para el usuario {UserId}: {ErrorMessage}",
                    paymentDto.SourceAccountNumber, paymentDto.CreditCardNumber, userId, ex.Message);
                TempData["ErrorMessage"] = ex.Message;
                ViewBag.SourceAccounts = new SelectList(paymentDto.AvailableAccounts, "AccountNumber", "DisplayText", paymentDto.SourceAccountNumber);
                ViewBag.CreditCards = new SelectList(paymentDto.AvailableCreditCards, "CardId", "DisplayText", paymentDto.CreditCardNumber);
                return View(paymentDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al procesar el pago de tarjeta de crédito desde {SourceAccount} para tarjeta {CreditCardNumber} para el usuario {UserId}.",
                    paymentDto.SourceAccountNumber, paymentDto.CreditCardNumber, userId);
                TempData["ErrorMessage"] = "Ocurrió un error inesperado al procesar el pago de la tarjeta de crédito. Inténtalo de nuevo.";
                ViewBag.SourceAccounts = new SelectList(paymentDto.AvailableAccounts, "AccountNumber", "DisplayText", paymentDto.SourceAccountNumber);
                ViewBag.CreditCards = new SelectList(paymentDto.AvailableCreditCards, "CardId", "DisplayText", paymentDto.CreditCardNumber);
                return View(paymentDto);
            }
        }

        public IActionResult CreditCardPaymentConfirmation()
        {
            return View();
        }
        #endregion

        #region Pagos de Préstamos
        // GET: /Transaction/PayLoan
        public async Task<IActionResult> PayLoan()
        {
            var userId = GetCurrentUserId();
            var accounts = await _accountServiceForWebApp.GetSavingsAccountsByUserIdAsync(userId);
            var loans = await _accountServiceForWebApp.GetLoansByUserIdAsync(userId); // Asume que este método existe y retorna List<LoanDTO>

            var model = new LoanPaymentDto
            {
                AvailableAccounts = accounts.Where(a => a.IsActive).ToList(),
                AvailableLoans = loans.Where(l => l.IsActive).ToList() // Filtra solo préstamos activos
            };
            ViewBag.SourceAccounts = new SelectList(model.AvailableAccounts, "AccountNumber", "DisplayText");
            ViewBag.Loans = new SelectList(model.AvailableLoans, "LoanIdentifier", "DisplayText");
            return View(model);
        }

        // POST: /Transaction/PayLoan
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PayLoan(LoanPaymentDto paymentDto)
        {
            var userId = GetCurrentUserId();
            var accounts = await _accountServiceForWebApp.GetSavingsAccountsByUserIdAsync(userId);
            var loans = await _accountServiceForWebApp.GetLoansByUserIdAsync(userId);
            paymentDto.AvailableAccounts = accounts.Where(a => a.IsActive).ToList();
            paymentDto.AvailableLoans = loans.Where(l => l.IsActive).ToList();

            if (!ModelState.IsValid)
            {
                ViewBag.SourceAccounts = new SelectList(paymentDto.AvailableAccounts, "AccountNumber", "DisplayText", paymentDto.SourceAccountNumber);
                ViewBag.Loans = new SelectList(paymentDto.AvailableLoans, "LoanIdentifier", "DisplayText", paymentDto.LoanIdentifier);
                TempData["ErrorMessage"] = "Por favor, corrige los errores del formulario.";
                return View(paymentDto);
            }

            try
            {
                await _transactionService.PayLoanAsync(paymentDto);
                TempData["SuccessMessage"] = "Pago de préstamo realizado exitosamente.";
                _logger.LogInformation("Pago de préstamo exitoso desde {SourceAccount} para préstamo {LoanIdentifier} por {Amount:C} para el usuario {UserId}.",
                    paymentDto.SourceAccountNumber, paymentDto.LoanIdentifier, paymentDto.Amount, userId);
                return RedirectToAction("LoanPaymentConfirmation");
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Pago de préstamo fallido (validación) desde {SourceAccount} para préstamo {LoanIdentifier} para el usuario {UserId}: {ErrorMessage}",
                    paymentDto.SourceAccountNumber, paymentDto.LoanIdentifier, userId, ex.Message);
                TempData["ErrorMessage"] = ex.Message;
                ViewBag.SourceAccounts = new SelectList(paymentDto.AvailableAccounts, "AccountNumber", "DisplayText", paymentDto.SourceAccountNumber);
                ViewBag.Loans = new SelectList(paymentDto.AvailableLoans, "LoanIdentifier", "DisplayText", paymentDto.LoanIdentifier);
                return View(paymentDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al procesar el pago de préstamo desde {SourceAccount} para préstamo {LoanIdentifier} para el usuario {UserId}.",
                    paymentDto.SourceAccountNumber, paymentDto.LoanIdentifier, userId);
                TempData["ErrorMessage"] = "Ocurrió un error inesperado al procesar el pago del préstamo. Inténtalo de nuevo.";
                ViewBag.SourceAccounts = new SelectList(paymentDto.AvailableAccounts, "AccountNumber", "DisplayText", paymentDto.SourceAccountNumber);
                ViewBag.Loans = new SelectList(paymentDto.AvailableLoans, "LoanIdentifier", "DisplayText", paymentDto.LoanIdentifier);
                return View(paymentDto);
            }
        }

        public IActionResult LoanPaymentConfirmation()
        {
            return View();
        }
        #endregion

        #region Pagos a Beneficiarios
        // GET: /Transaction/PayBeneficiary
        public async Task<IActionResult> PayBeneficiary()
        {
            var userId = GetCurrentUserId();
            var accounts = await _accountServiceForWebApp.GetSavingsAccountsByUserIdAsync(userId);
            var beneficiaries = await _beneficiaryService.GetBeneficiariesByUserIdAsync(userId); // Asume que este método existe en IBeneficiaryService y retorna List<BeneficiaryDTO>

            var model = new PayBeneficiaryDTO
            {
                AvailableAccounts = accounts.Where(a => a.IsActive).ToList(),
                AvailableBeneficiaries = beneficiaries.ToList() // Asume que BeneficiaryDTO no tiene 'IsActive' o ya está filtrado
            };
            ViewBag.SourceAccounts = new SelectList(model.AvailableAccounts, "AccountNumber", "DisplayText");
            ViewBag.Beneficiaries = new SelectList(model.AvailableBeneficiaries, "Id", "DisplayText");
            return View(model);
        }

        // POST: /Transaction/PayBeneficiary
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PayBeneficiary(PayBeneficiaryDTO dto)
        {
            var userId = GetCurrentUserId();
            var accounts = await _accountServiceForWebApp.GetSavingsAccountsByUserIdAsync(userId);
            var beneficiaries = await _beneficiaryService.GetBeneficiariesByUserIdAsync(userId);
            dto.AvailableAccounts = accounts.Where(a => a.IsActive).ToList();
            dto.AvailableBeneficiaries = beneficiaries.ToList();

            if (!ModelState.IsValid)
            {
                ViewBag.SourceAccounts = new SelectList(dto.AvailableAccounts, "AccountNumber", "DisplayText", dto.SourceAccountNumber);
                ViewBag.Beneficiaries = new SelectList(dto.AvailableBeneficiaries, "Id", "DisplayText", dto.BeneficiaryId);
                TempData["ErrorMessage"] = "Por favor, corrige los errores del formulario.";
                return View(dto);
            }

            try
            {
                await _transactionService.ExecutePayBeneficiaryTransactionAsync(dto);
                TempData["SuccessMessage"] = "Pago a beneficiario realizado exitosamente.";
                _logger.LogInformation("Pago a beneficiario exitoso desde {SourceAccount} al beneficiario {BeneficiaryId} por {Amount:C} para el usuario {UserId}.",
                   dto.SourceAccountNumber, dto.BeneficiaryId, dto.Amount, userId);
                return RedirectToAction("BeneficiaryPaymentConfirmation");
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Pago a beneficiario fallido (validación) desde {SourceAccount} al beneficiario {BeneficiaryId} para el usuario {UserId}: {ErrorMessage}",
                    dto.SourceAccountNumber, dto.BeneficiaryId, userId, ex.Message);
                TempData["ErrorMessage"] = ex.Message;
                ViewBag.SourceAccounts = new SelectList(dto.AvailableAccounts, "AccountNumber", "DisplayText", dto.SourceAccountNumber);
                ViewBag.Beneficiaries = new SelectList(dto.AvailableBeneficiaries, "Id", "DisplayText", dto.BeneficiaryId);
                return View(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al procesar el pago a beneficiario desde {SourceAccount} al beneficiario {BeneficiaryId} para el usuario {UserId}.",
                    dto.SourceAccountNumber, dto.BeneficiaryId, userId);
                TempData["ErrorMessage"] = "Ocurrió un error inesperado al procesar el pago a beneficiario. Inténtalo de nuevo.";
                ViewBag.SourceAccounts = new SelectList(dto.AvailableAccounts, "AccountNumber", "DisplayText", dto.SourceAccountNumber);
                ViewBag.Beneficiaries = new SelectList(dto.AvailableBeneficiaries, "Id", "DisplayText", dto.BeneficiaryId);
                return View(dto);
            }
        }

        public IActionResult BeneficiaryPaymentConfirmation()
        {
            return View();
        }
        #endregion
    }
}