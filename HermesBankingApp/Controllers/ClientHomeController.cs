using HermesBanking.Core.Application.DTOs.Transaction;
using HermesBanking.Core.Application.Interfaces;
using HermesBanking.Core.Application.Services;
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
        private readonly ITransactionService _transactionService;
        public ClientHomeController(
            UserManager<AppUser> userManager,
            ISavingsAccountRepository accountRepo,
            ITransactionRepository transactionRepo, ITransactionService transaction)
        {
            _userManager = userManager;
            _accountRepo = accountRepo;
            _transactionRepo = transactionRepo;
            _transactionService = transaction;
        }

        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User)!;

            var accounts = await _accountRepo
                .GetAllQuery()
                .Where(a => a.ClientId == userId && a.IsActive)
                .ToListAsync();

            var savingsAccountTransactions = await _transactionRepo
                .GetAllQuery()
                .Where(t => accounts.Select(a => a.Id.ToString()).Contains(t.SourceAccountId) ||
                            accounts.Select(a => a.Id.ToString()).Contains(t.DestinationAccountId))
                .OrderByDescending(t => t.TransactionDate)
                .Take(20) 
                .Select(t => new TransactionDTO 
                {
                    Id = t.Id,
                    Amount = t.Amount,
                    TransactionDate = t.TransactionDate ?? DateTime.MinValue,
                    TransactionType = t.TransactionType,
                    Description = t.Description,
                    SourceAccountId = t.SourceAccountId,
                    DestinationAccountId = t.DestinationAccountId,
                    DestinationCardId = t.DestinationCardId,
                    DestinationLoanId = t.DestinationLoanId,
                    Type = t.TransactionType.ToString(),
                    Origin = t.SourceAccountId, 
                    Beneficiary = t.DestinationAccountId 
                })
                .ToListAsync();

            var otherServiceTransactions = await _transactionService.GetClientServiceTransactionsAsync(userId);

            ViewBag.Accounts = accounts;
            ViewBag.SavingsAccountTransactions = savingsAccountTransactions;
            ViewBag.OtherServiceTransactions = otherServiceTransactions;

            return View();
        }
    }
}