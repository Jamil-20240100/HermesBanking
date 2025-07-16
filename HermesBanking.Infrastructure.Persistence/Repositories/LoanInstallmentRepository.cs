using HermesBanking.Core.Domain.Entities;
using HermesBanking.Core.Domain.Interfaces;
using HermesBanking.Infrastructure.Persistence.Contexts;

namespace HermesBanking.Infrastructure.Persistence.Repositories
{
    public class AmortizationLInstallmentRepository : GenericRepository<AmortizationInstallment>, IAmortizationInstallmentRepository
    {
        public AmortizationLInstallmentRepository(HermesBankingContext context) : base(context)
        {
        }
    }
}
