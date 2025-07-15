using HermesBanking.Core.Application.Interfaces;
using HermesBanking.Core.Domain.Entities;
using HermesBanking.Core.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HermesBanking.Infrastructure.Application.Services
{
    public class TransactionService : ITransactionService
    {
        private readonly ITransactionRepository _transactionRepo;
        private readonly IUserService _userService;

        public TransactionService(ITransactionRepository transactionRepo, IUserService userService)
        {
            _transactionRepo = transactionRepo;
            _userService = userService;
        }

        public async Task RegisterTransactionAsync(int savingsAccountId, string type, decimal amount, string origin, string beneficiary, string? cashierId = null)
        {
            var transaction = new Transaction
            {
                Type = type,
                Amount = amount,
                Origin = origin,
                Beneficiary = beneficiary,
                Date = DateTime.Now,
                SavingsAccountId = savingsAccountId,
                PerformedByCashierId = cashierId
            };

            await _transactionRepo.AddAsync(transaction);  // Cambiar _transactionRepository a _transactionRepo
        }

        public async Task<List<Transaction>> GetTransactionsByCashierAndDateAsync(string cashierId, DateTime date)
        {
            return await _transactionRepo  // Cambiar _transactionRepository a _transactionRepo
                .GetAllQuery()
                .Where(t => t.PerformedByCashierId == cashierId && t.Date.Date == date.Date)
                .ToListAsync();
        }
    }

}
