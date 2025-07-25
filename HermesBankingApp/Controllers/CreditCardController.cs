using AutoMapper;
using HermesBanking.Core.Application.DTOs.CreditCard;
using HermesBanking.Core.Application.DTOs.Email;
using HermesBanking.Core.Application.Interfaces;
using HermesBanking.Core.Application.Services;
using HermesBanking.Core.Application.ViewModels.CreditCard;
using HermesBanking.Core.Application.ViewModels.User;
using HermesBanking.Core.Domain.Common.Enums;
using HermesBanking.Core.Domain.Interfaces;
using HermesBanking.Infrastructure.Identity.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using System.Text;

namespace HermesBankingApp.Controllers
{
    public class CreditCardController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IAccountServiceForWebApp _accountServiceForWebApp;
        private readonly ICreditCardService _service;
        private readonly IMapper _mapper;
        private readonly IEmailService _emailService;
        private readonly ILoanService _loanService;
        private readonly ITransactionService _transactionService;
        private readonly ICreditCardService _creditCardService;
        private readonly ITransactionRepository _transactionRepo;
        private readonly ICommerceService _commerceService;

        public CreditCardController(ICreditCardService service,ICommerceService commerceService, IMapper mapper, ITransactionRepository transactionRepository,UserManager<AppUser> userManager, IAccountServiceForWebApp accountServiceForWebApp, IEmailService emailService, ILoanService loanService, ITransactionService transactionService, ICreditCardService creditCardService)
        {
            _service = service;
            _mapper = mapper;
            _userManager = userManager;
            _accountServiceForWebApp = accountServiceForWebApp;
            _emailService = emailService;
            _loanService = loanService;
            _transactionService = transactionService;
            _creditCardService = creditCardService;
            _transactionRepo = transactionRepository;
            _commerceService = commerceService;
        }

        public async Task<IActionResult> Index()
        {
            //
            // user validation
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

            var pagedResponse = await _service.GetCreditCardsAsync(); // Llama a tu método específico
            var vmList = _mapper.Map<List<CreditCardViewModel>>(pagedResponse.Data); // Mapea el Data de la respuesta paginada

            // El ordenamiento que tenías en el controlador
            vmList.Sort((a, b) =>
            {
                if (a.IsActive && !b.IsActive) return -1;
                if (!a.IsActive && b.IsActive) return 1;
                return b.CreatedAt.CompareTo(a.CreatedAt);
            });

            return View(vmList);
        }

        public IActionResult CreateForm(string clientId)
        {
            ViewBag.ClientId = clientId;
            return View();
        }

        public async Task<IActionResult> SelectClient(string? cedula)
        {
            var clients = await _accountServiceForWebApp.GetAllActiveUserByRole(Roles.Client.ToString());

            if (!string.IsNullOrWhiteSpace(cedula))
                clients = clients.Where(c => c.UserId.Contains(cedula)).ToList();

            foreach (var client in clients)
            {
                client.TotalDebt = await _loanService.GetCurrentDebtForClient(client.Id);
            }

            decimal deudaPromedio = clients.Count != 0
                ? clients.Average(c => c.TotalDebt ?? 0)
                : 0;

            ViewBag.DeudaPromedio = deudaPromedio;

            var vms = _mapper.Map<List<UserViewModel>>(clients);
            return View(vms);
        }

        [HttpPost]
        public async Task<IActionResult> SelectClient(string? clientId, string? cedula)
        {
            AppUser? userSession = await _userManager.GetUserAsync(User);
            if (userSession == null)
                return RedirectToRoute(new { controller = "Login", action = "Index" });

            if (string.IsNullOrEmpty(clientId))
            {
                ModelState.AddModelError("", "Debe seleccionar un cliente para continuar.");
                var clients = await _accountServiceForWebApp.GetAllActiveUserByRole(Roles.Client.ToString());

                if (!string.IsNullOrWhiteSpace(cedula))
                    clients = clients.Where(c => c.UserId.Contains(cedula)).ToList();

                decimal deudaPromedio = clients.Count != 0
                    ? clients.Average(c => c.InitialAmount ?? 0)
                    : 0;

                ViewBag.DeudaPromedio = deudaPromedio;

                var vms = _mapper.Map<List<UserViewModel>>(clients);
                return View(vms);
            }

            return RedirectToAction("CreateForm", new { clientId });
        }

        [HttpPost]
        public async Task<IActionResult> Create(SaveCreditCardViewModel vm)
        {
            AppUser? userSession = await _userManager.GetUserAsync(User);
            if (userSession == null)
                return RedirectToRoute(new { controller = "Login", action = "Index" });

            var user = await _accountServiceForWebApp.GetUserByUserName(userSession.UserName ?? "");
            if (user == null)
                return RedirectToRoute(new { controller = "Login", action = "Index" });

            vm.CreatedByAdminId = userSession.Id;
            vm.AdminFullName = $"{userSession.Name} {userSession.LastName}";

            if (string.IsNullOrEmpty(vm.ClientId))
            {
                ModelState.AddModelError("", "No se ha proporcionado el ID del cliente para asignar la tarjeta.");
                return RedirectToRoute(new { controller = "CreditCard", action = "SelectClient" });
            }

            var clientUser = await _accountServiceForWebApp.GetUserById(vm.ClientId);
            if (clientUser == null)
            {
                ModelState.AddModelError("", "Cliente no encontrado con el ID proporcionado.");
                return RedirectToRoute(new { controller = "CreditCard", action = "SelectClient" });
            }

            vm.ClientFullName = $"{clientUser.Name} {clientUser.LastName}";
            vm.ExpirationDate = DateTime.Now.AddYears(3);
            vm.CardId = _service.GenerateUniqueCardId();
            vm.CVC = _service.GenerateAndEncryptCVC();
            vm.TotalOwedAmount = 0;
            vm.IsActive = true;

            if (!ModelState.IsValid)
            {
                return View("CreateForm", vm);
            }

            var dto = _mapper.Map<CreditCardDTO>(vm);
            await _service.AddAsync(dto);

            return RedirectToRoute(new { controller = "CreditCard", action = "Index" });
        }

        public async Task<IActionResult> Edit(int id)
        {

            //
            // user validation
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

            if (!ModelState.IsValid)
                return RedirectToRoute(new { controller = "CreditCard", action = "Index" });
            var card = await _service.GetById(id);

            if (card == null)
                return RedirectToRoute(new { controller = "CreditCard", action = "Index" });

            var vm = _mapper.Map<SaveCreditCardViewModel>(card);

            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(SaveCreditCardViewModel vm)
        {
            //
            // user validation
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

            if (!ModelState.IsValid)
                return View(vm);

            var checkDTO = await _service.GetById(vm.Id);

            if (checkDTO == null)
            {
                ViewData["ErrorMessage"] = "Tarjeta no encontrada.";
                return View(vm);
            }

            if (vm.CreditLimit < checkDTO.TotalOwedAmount)
            {
                ViewData["ErrorMessage"] = "No se pudo actualizar el límite porque el nuevo valor es menor a la deuda.";
                return View(vm);
            }

            checkDTO.CreditLimit = vm.CreditLimit;

            await _service.UpdateAsync(checkDTO, checkDTO.Id);
            var client = await _accountServiceForWebApp.GetUserEmailAsync(checkDTO.ClientId);
            string last4Digits = checkDTO.CardId.Substring(checkDTO.CardId.Length - 4);

            if (client != null)
            {
                await _emailService.SendAsync(new EmailRequestDto()
                {
                    To = client,
                    HtmlBody = $"Su tarjeta terminada en [{last4Digits}] ha presentado una actualización en el límite de crédito disponible.",
                    Subject = "Límite de Crédito actualizado"
                });
            }

            return RedirectToRoute(new { controller = "CreditCard", action = "Index" });
        }

        public async Task<IActionResult> ConfirmCancel(int id)
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

            var card = await _service.GetById(id);

            var vm = new CancelCreditCardViewModel
            {
                Id = card.Id,
            };

            string last4Digits = card.CardId.Substring(card.CardId.Length - 4);
            ViewBag.Last4Digits = last4Digits;

            return View("ConfirmCancel", vm);
        }

        [HttpPost]
        public async Task<IActionResult> Cancel(int id)
        {
            try
            {
                var card = await _service.GetById(id);
                if (card == null)
                {
                    TempData["ErrorMessage"] = "Tarjeta no encontrada.";
                    return RedirectToRoute(new { controller = "CreditCard", action = "Index" });
                }

                if (card.TotalOwedAmount > 0)
                {
                    TempData["ErrorMessage"] = "Para cancelar esta tarjeta, el cliente debe saldar la totalidad de la deuda pendiente.";
                    return RedirectToRoute(new { controller = "CreditCard", action = "Index" });
                }

                //desactivate card
                card.IsActive = false;

                await _service.UpdateAsync(card, card.Id);
                var client = await _accountServiceForWebApp.GetUserEmailAsync(card.ClientId);
                string last4Digits = card.CardId.Substring(card.CardId.Length - 4);

                if (client != null)
                {
                    await _emailService.SendAsync(new EmailRequestDto()
                    {
                        To = client,
                        HtmlBody = $"Su tarjeta terminada en [{last4Digits}] ha sido cancelada.",
                        Subject = "Tarjeta cancelada"
                    });
                }
                TempData["Success"] = "La tarjeta fue cancelada correctamente.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Details(int cardId, string cardNumber, int pagina = 1, int pageSize = 10)
        {
            if (cardId == 0)
            {
                return NotFound("Card ID is missing or invalid.");
            }

            // Obtener los detalles de la tarjeta de crédito
            var creditCardResponse = await _creditCardService.GetCreditCardsAsync();
            var creditCard = creditCardResponse?.Data?.FirstOrDefault(c => c.Id == cardId); // Usamos cardId directamente (String)

            if (creditCard == null)
            {
                return NotFound("Credit card not found");
            }

            // Obtener todas las transacciones asociadas a la tarjeta de crédito
            var allTransactions = await _transactionService.GetAllTransactionsAsync();

            //var allCommerce = await _commerceService.GetCommerceByIdAsync();

            // Filtramos las transacciones relacionadas con esta tarjeta
            var cardTransactions = allTransactions
                .Where(t => t.DestinationCardId == cardId.ToString() || t.CreditCardId == cardNumber) // Filtramos por el cardId de las transacciones
                .OrderByDescending(t => t.TransactionDate) // Ordenamos por fecha de transacción
                .Skip((pagina - 1) * pageSize) // Paginación: saltamos a la página correcta
                .Take(pageSize) // Tomamos solo los registros de la página actual
                .ToList();

            var lastTransaction = cardTransactions
                .OrderByDescending(t => t.TransactionDate)
                .FirstOrDefault();

            // Calculamos el total de transacciones
            var totalTransactions = allTransactions.Count(t => t.DestinationCardId == cardId.ToString() || t.CreditCardId == cardNumber);
            var totalPages = (int)Math.Ceiling(totalTransactions / (double)pageSize);

            // Preparamos la paginación
            var pagination = new PaginationDTO
            {
                PaginaActual = pagina,
                TotalPaginas = totalPages,
                TotalRegistros = totalTransactions
            };

            // Crear un ViewModel para pasar a la vista
            var creditCardDetailsViewModel = new CreditCardDetailsViewModel
            {
                CreditCardId = creditCard.Id,
                CardId = creditCard.CardId,
                ClientFullName = creditCard.ClientFullName,
                CreditLimit = creditCard.CreditLimit,
                TotalOwedAmount = creditCard.TotalOwedAmount,
                ExpirationDate = lastTransaction?.TransactionDate.ToString("MM/yy") ?? "N/A",
                Transactions = cardTransactions, // Lista de transacciones asociadas a la tarjeta
                Pagination = pagination, // Paginación
                CreditCard = creditCard // Asignamos el CreditCardDTO aquí
                
            };

            return View(creditCardDetailsViewModel); // Pasamos el ViewModel a la vista
        }






    }
}