using HermesBanking.Core.Application.Interfaces;
using HermesBanking.Core.Domain.Entities;
using HermesBanking.Core.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HermesBanking.Infrastructure.Identity.Services
{
    public class TransactionService : ITransactionService
    {
        private readonly ITransactionRepository _transactionRepository;

        public TransactionService(ITransactionRepository transactionRepository)
        {
            _transactionRepository = transactionRepository;
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

            await _transactionRepository.AddAsync(transaction);
        }

        public async Task<List<Transaction>> GetTransactionsByCashierAndDateAsync(string cashierId, DateTime date)
        {
            return await _transactionRepository
                .GetAllQuery()
                .Where(t => t.PerformedByCashierId == cashierId && t.Date.Date == date.Date)
                .ToListAsync();
        }

    }
}
