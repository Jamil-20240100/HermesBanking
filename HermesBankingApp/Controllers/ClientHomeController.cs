using HermesBanking.Core.Domain.Common.Enums;
using HermesBanking.Core.Domain.Interfaces;
using HermesBanking.Infrastructure.Identity.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace HermesBankingApp.Controllers
{
    [Authorize(Roles = "Client")]
    public class ClientHomeController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly ISavingsAccountRepository _accountRepo;
        private readonly ITransactionRepository _transactionRepo;

        public ClientHomeController(
            UserManager<AppUser> userManager,
            ISavingsAccountRepository accountRepo,
            ITransactionRepository transactionRepo)
        {
            _userManager = userManager;
            _accountRepo = accountRepo;
            _transactionRepo = transactionRepo;
        }

        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User)!;

            // Obtener cuentas activas del cliente
            var accounts = await _accountRepo
                .GetAllQuery()
                .Where(a => a.ClientId == userId && a.IsActive)
                .ToListAsync();

            // Obtener las transacciones relacionadas con las cuentas activas
            var transactions = await _transactionRepo
                .GetAllQuery()
                .Where(t => accounts.Select(a => a.Id.ToString()).Contains(t.SavingsAccountId.ToString()) ||
                            t.TransactionType == TransactionType.PagoPrestamo ||
                            t.TransactionType == TransactionType.PagoTarjetaCredito)  // Incluir pagos de tarjeta de crédito y préstamos
                .OrderByDescending(t => t.TransactionDate)
                .Take(20)
                .ToListAsync();

            // Asignar el tipo adecuado para transacciones de tarjeta de crédito y pago de préstamo
            foreach (var t in transactions)
            {
                if (t.TransactionType == TransactionType.PagoTarjetaCredito)
                {
                    t.Type = "Pago de Tarjeta de Crédito"; // Asignar un tipo personalizado para tarjeta de crédito
                }
                else if (t.TransactionType == TransactionType.PagoPrestamo)
                {
                    t.Type = "Pago de Préstamo"; // Asignar un tipo personalizado para pago de préstamo
                }
            }

            ViewBag.Accounts = accounts;
            ViewBag.Transactions = transactions;

            return View();
        }

    }
}