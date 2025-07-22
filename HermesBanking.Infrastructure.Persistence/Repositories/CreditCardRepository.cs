using HermesBanking.Core.Domain.Entities;
using HermesBanking.Core.Domain.Interfaces;
using HermesBanking.Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace HermesBanking.Infrastructure.Persistence.Repositories
{
    public class CreditCardRepository : GenericRepository<CreditCard>, ICreditCardRepository
    {
        public CreditCardRepository(HermesBankingContext context) : base(context) { }

        public async Task<CreditCard?> GetByCardNumberAsync(string cardNumber)
        {
            return await _context.CreditCards
                .FirstOrDefaultAsync(card => card.CardId == cardNumber);  // Cambiar 'CardNumber' por 'CardId'
        }

      
    }
}
