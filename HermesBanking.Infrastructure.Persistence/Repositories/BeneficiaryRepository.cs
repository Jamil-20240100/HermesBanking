using HermesBanking.Core.Domain.Entities;
using HermesBanking.Core.Domain.Interfaces;
using HermesBanking.Infrastructure.Persistence.Contexts;

namespace HermesBanking.Infrastructure.Persistence.Repositories
{
    public class BeneficiaryRepository : GenericRepository<Beneficiary>, IBeneficiaryRepository
    {
        public BeneficiaryRepository(HermesBankingContext context) : base(context)
        {
        }
    }
}