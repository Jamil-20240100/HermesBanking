using HermesBanking.Core.Domain.Entities;
using HermesBanking.Core.Domain.Interfaces;
using HermesBanking.Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System.Linq.Expressions;

namespace HermesBanking.Infrastructure.Persistence.Repositories
{
    public class TransactionRepository : GenericRepository<Transaction>, ITransactionRepository
    {
        private readonly HermesBankingContext _context;

        public TransactionRepository(HermesBankingContext context) : base(context)
        {
            _context = context;
        }

        public async Task<Transaction> GetByIdAsync(int transactionId)
        {
            return await _context.Transactions
                                 .FirstOrDefaultAsync(t => t.Id == transactionId);
        }

        public async Task<IEnumerable<Transaction>> GetAllAsync()
        {
            return await _context.Transactions.ToListAsync();
        }

        public async Task<IEnumerable<Transaction>> GetByConditionAsync(Expression<Func<Transaction, bool>> expression)
        {
            if (expression == null)
                throw new ArgumentNullException(nameof(expression), "Expression cannot be null.");

            return await _context.Transactions.Where(expression).ToListAsync();
        }

        public async Task<IEnumerable<Transaction>> GetByCashierId(string cashierId, DateTime? today)
        {
            if (today != null)
            {
                var start = today.Value.Date;
                var end = start.AddDays(1);

                return await _context.Transactions
                    .Where(t => t.CashierId == cashierId && t.TransactionDate >= start && t.TransactionDate < end)
                    .ToListAsync();
            }
            else
            {
                return await _context.Transactions
                    .Where(t => t.CashierId == cashierId)
                    .ToListAsync();
            }
        }

        // 🆕 Implementación para control transaccional manual
        public async Task<IDbContextTransaction> BeginTransactionAsync()
        {
            return await _context.Database.BeginTransactionAsync();
        }


        //HERMES PAY 
        public async Task<List<Transaction>> GetByConditionAsyncForHermesPay(Expression<Func<Transaction, bool>> predicate)
        {
            return await _context.Transactions
                                 .Where(predicate)
                                 .ToListAsync();
        }
    }
}