using AutoMapper;
using HermesBanking.Core.Application.DTOs.CreditCard;
using HermesBanking.Core.Application.DTOs.Email;
using HermesBanking.Core.Application.DTOs.Transaction;
using HermesBanking.Core.Application.Interfaces;
using HermesBanking.Core.Application.ViewModels.CreditCard;
using HermesBanking.Core.Application.ViewModels.User;
using HermesBanking.Core.Domain.Common.Enums;
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
        public CreditCardController(
            ICreditCardService service,
            IMapper mapper,
            UserManager<AppUser> userManager,
            IAccountServiceForWebApp accountServiceForWebApp,
            IEmailService emailService,
            ILoanService loanService,
            ITransactionService transactionService) 
        {
            _service = service;
            _mapper = mapper;
            _userManager = userManager;
            _accountServiceForWebApp = accountServiceForWebApp;
            _emailService = emailService;
            _loanService = loanService;
            _transactionService = transactionService; 
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

            var allCards = await _service.GetAll();
            var vmList = _mapper.Map<List<CreditCardViewModel>>(allCards);

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

            decimal deudaPromedio = clients.Any()
                ? clients.Average(c => c.TotalDebt)
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

                decimal deudaPromedio = clients.Any()
                    ? clients.Average(c => c.InitialAmount ?? 0)
                    : 0;

                ViewBag.DeudaPromedio = deudaPromedio;

                var vms = _mapper.Map<List<UserViewModel>>(clients);
                return View(vms);
            }

            return RedirectToAction("CreateForm", new { clientId = clientId });
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
            vm.CardId = GenerateUniqueCardId();
            vm.CVC = GenerateAndEncryptCVC();
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

        private string GenerateUniqueCardId()
        {
            Random random = new Random();
            string cardId;
            do
            {
                cardId = "";
                for (int i = 0; i < 16; i++)
                {
                    cardId += random.Next(0, 10).ToString();
                }
            } while (false);
            return cardId;
        }

        private string GenerateAndEncryptCVC()
        {
            Random random = new Random();

            string cvc = random.Next(100, 1000).ToString();

            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(cvc));

                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }

        [HttpPost]
        public async Task<IActionResult> PayCreditCard(string accountNumber, string cardNumber, decimal amount, string cashierId)
        {
            var account = await _accountServiceForWebApp.GetAccountByNumberAsync(accountNumber);
            var card = await _service.GetCardByNumberAsync(cardNumber);

            if (account == null || card == null)
            {
                ModelState.AddModelError("", "La cuenta o tarjeta no existen.");
                return View();
            }

            if (account.Balance < amount)
            {
                ModelState.AddModelError("", "Fondos insuficientes.");
                return View();
            }

            if (amount > card.TotalOwedAmount)
            {
                amount = card.TotalOwedAmount; // Solo se puede pagar la deuda total
            }

            // Debitar de la cuenta de origen
            account.Balance -= amount;
            await _accountServiceForWebApp.UpdateAsync(account);

            // Reducir la deuda de la tarjeta de crédito
            card.TotalOwedAmount -= amount;
            await _service.UpdateAsync(card, card.Id);

            // Crear el DTO de la transacción
            var transactionDTO = new TransactionDTO
            {
                SavingsAccountId = account.Id,
                Type = "CRÉDITO", // Asignar un tipo de transacción
                Amount = amount,
                Origin = accountNumber,    // Origen
                Beneficiary = cardNumber,  // Beneficiario
                CashierId = cashierId,     // ID del cajero
                Date = DateTime.Now        // Fecha de la transacción
            };

            // Registrar la transacción (pasando el DTO como parámetro)
            await _transactionService.RegisterTransactionAsync(transactionDTO);

            // Enviar correos electrónicos
            var user = await _accountServiceForWebApp.GetUserById(account.ClientId);
            string last4Card = card.CardId.Substring(card.CardId.Length - 4);
            string last4Account = account.AccountNumber.Substring(account.AccountNumber.Length - 4);

            await _emailService.SendAsync(new EmailRequestDto
            {
                To = user.Email,
                Subject = $"Pago realizado a la tarjeta {last4Card}",
                HtmlBody = $"Se ha realizado un pago de RD${amount:N2} desde su cuenta {last4Account} a su tarjeta de crédito."
            });

            return RedirectToAction("Home", "Client");
        }


    }
}