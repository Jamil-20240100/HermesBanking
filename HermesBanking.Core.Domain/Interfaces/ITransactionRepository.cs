using HermesBanking.Core.Domain.Entities;

namespace HermesBanking.Core.Domain.Interfaces
{
    public interface ITransactionRepository : IGenericRepository<Transaction>
    {
        Task AddAsync(Transaction transaction, string cashierId);
        Task<Transaction> GetByIdAsync(int transactionId);
        Task<IEnumerable<Transaction>> GetAllAsync();
        Task UpdateAsync(Transaction transaction);

    }
}
