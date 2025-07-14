using HermesBanking.Core.Domain.Entities;
using HermesBanking.Core.Domain.Interfaces;
using HermesBanking.Infrastructure.Persistence.Contexts;

namespace HermesBanking.Infrastructure.Persistence.Repositories
{
    public class TransactionRepository : GenericRepository<Transaction>, ITransactionRepository
    {
        public TransactionRepository(HermesBankingContext context) : base(context) { }
    }
}
