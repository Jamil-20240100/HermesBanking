using HermesBanking.Core.Domain.Entities;

namespace HermesBanking.Core.Domain.Interfaces
{
    public interface ICreditCardRepository : IGenericRepository<CreditCard>
    {
        Task<CreditCard?> GetByCardNumberAsync(string cardNumber);
        

    }
}
