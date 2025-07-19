using HermesBanking.Core.Domain.Entities;
using HermesBanking.Core.Domain.Interfaces;
using HermesBanking.Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
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

        // Implementación del método AddAsync
        public async Task AddAsync(Transaction transaction, string cashierId)
        {
            if (transaction == null)
                throw new ArgumentNullException(nameof(transaction), "Transaction cannot be null.");

            if (string.IsNullOrEmpty(cashierId))
                throw new ArgumentException("CashierId cannot be null or empty.");

            // Asignar el CashierId
            transaction.CashierId = cashierId;

            await _context.Transactions.AddAsync(transaction);
            await _context.SaveChangesAsync();
        }

        // Implementación del método GetByIdAsync
        public async Task<Transaction> GetByIdAsync(int transactionId)
        {
            return await _context.Transactions
                                 .FirstOrDefaultAsync(t => t.Id == transactionId);
        }

        // Implementación del método GetAllAsync
        public async Task<IEnumerable<Transaction>> GetAllAsync()
        {
            return await _context.Transactions.ToListAsync();
        }

        // Implementación del método UpdateAsync
        public async Task UpdateAsync(Transaction transaction)
        {
            if (transaction == null)
                throw new ArgumentNullException(nameof(transaction), "Transaction cannot be null.");

            _context.Transactions.Update(transaction);
            await _context.SaveChangesAsync();
        }

        // Implementación del método UpdateAsync (con ID)
        public async Task<Transaction> UpdateAsync(int id, Transaction transaction)
        {
            var existingEntity = await _context.Transactions.FindAsync(id);
            if (existingEntity != null)
            {
                _context.Entry(existingEntity).CurrentValues.SetValues(transaction);
                await _context.SaveChangesAsync();
                return existingEntity;
            }
            return null;
        }

        // Implementación del método DeleteAsync por entidad
        public async Task DeleteAsync(Transaction transaction)
        {
            if (transaction == null)
                throw new ArgumentNullException(nameof(transaction), "Transaction cannot be null.");

            _context.Transactions.Remove(transaction);
            await _context.SaveChangesAsync();
        }

        // Implementación del método DeleteAsync por id
        public async Task DeleteAsync(int? id)
        {
            if (id == null)
                throw new ArgumentNullException(nameof(id), "Id cannot be null.");

            var entity = await _context.Transactions.FindAsync(id);
            if (entity != null)
            {
                _context.Transactions.Remove(entity);
                await _context.SaveChangesAsync();
            }
        }

        // Implementación del método GetAllWithInclude
        public async Task<List<Transaction>> GetAllWithInclude(List<string> properties)
        {
            var query = _context.Transactions.AsQueryable();

            foreach (var property in properties)
            {
                query = query.Include(property);
            }

            return await query.ToListAsync();
        }

        // Implementación del método GetAllQuery
        public IQueryable<Transaction> GetAllQuery()
        {
            return _context.Transactions.AsQueryable();
        }

        // Implementación del método GetAllQueryWithInclude
        public IQueryable<Transaction> GetAllQueryWithInclude(List<string> properties)
        {
            var query = _context.Transactions.AsQueryable();

            foreach (var property in properties)
            {
                query = query.Include(property);
            }
            return query;
        }

        // Implementación del método GetByConditionAsync
        public async Task<IEnumerable<Transaction>> GetByConditionAsync(Expression<Func<Transaction, bool>> expression)
        {
            if (expression == null)
                throw new ArgumentNullException(nameof(expression), "Expression cannot be null.");

            return await _context.Transactions.Where(expression).ToListAsync();
        }
    }
}
