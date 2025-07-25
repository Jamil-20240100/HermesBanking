using HermesBanking.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore.Storage;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace HermesBanking.Core.Domain.Interfaces
{
    public interface ITransactionRepository : IGenericRepository<Transaction>
    {
        Task<Transaction> GetByIdAsync(int transactionId);
        Task<IEnumerable<Transaction>> GetAllAsync();
        Task<IEnumerable<Transaction>> GetByConditionAsync(Expression<Func<Transaction, bool>> expression);
        Task<IEnumerable<Transaction>> GetByCashierId(string cashierId, DateTime? today);

        // 🆕 Para manejar transacciones manuales
        Task<IDbContextTransaction> BeginTransactionAsync();

        Task<List<Transaction>> GetByConditionAsyncForHermesPay(Expression<Func<Transaction, bool>> predicate);

    }
}