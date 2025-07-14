using HermesBanking.Core.Domain.Entities;

namespace HermesBanking.Core.Domain.Interfaces
{
    public interface ISavingsAccountRepository : IGenericRepository<SavingsAccount>
    {
        Task<SavingsAccount?> GetByAccountNumberAsync(string accountNumber);
    }
}
