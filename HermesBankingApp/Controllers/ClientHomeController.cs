﻿using HermesBanking.Core.Domain.Interfaces;
using HermesBanking.Infrastructure.Identity.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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

            var accounts = await _accountRepo
                .GetAllQuery()
                .Where(a => a.ClientId == userId && a.IsActive)
                .ToListAsync();

            var transactions = await _transactionRepo
                .GetAllQuery()
                .Where(t => accounts.Select(a => a.Id).Contains(t.SavingsAccountId))
                .OrderByDescending(t => t.Date)
                .Take(20)
                .ToListAsync();

            ViewBag.Accounts = accounts;
            ViewBag.Transactions = transactions;

            return View();
        }
    }
}
