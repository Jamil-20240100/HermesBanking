using HermesBanking.Core.Domain.Entities;
using HermesBanking.Core.Domain.Interfaces;
using HermesBanking.Infrastructure.Persistence.Contexts;

namespace HermesBanking.Infrastructure.Persistence.Repositories
{
    public class CreditCardRepository : GenericRepository<CreditCard>, ICreditCardRepository
    {
        public CreditCardRepository(HermesBankingContext context) : base(context)
        {
        }
    }
}
