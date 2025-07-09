using HermesBanking.Core.Domain.Entities;
using HermesBanking.Core.Domain.Interfaces;
using HermesBanking.Infrastructure.Persistence.Contexts;

namespace HermesBanking.Infrastructure.Persistence.Repositories
{
    public class SavingsAccountRepository : GenericRepository<SavingsAccount>, ISavingsAccountRepository
    {
        public SavingsAccountRepository(HermesBankingContext context) : base(context)
        {
        }
    }
}
