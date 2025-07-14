using HermesBanking.Core.Domain.Entities;
using HermesBanking.Core.Domain.Interfaces;
using HermesBanking.Infrastructure.Persistence.Contexts;

namespace HermesBanking.Infrastructure.Persistence.Repositories
{
    public class LoanInstallmentRepository : GenericRepository<LoanInstallment>, ILoanInstallmentRepository
    {
        public LoanInstallmentRepository(HermesBankingContext context) : base(context)
        {
        }
    }
}
