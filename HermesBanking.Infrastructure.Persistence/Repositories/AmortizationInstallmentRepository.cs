using HermesBanking.Core.Domain.Interfaces;
using HermesBanking.Core.Domain.Entities;
using HermesBanking.Infrastructure.Persistence.Contexts;

namespace HermesBanking.Infrastructure.Persistence.Repositories
{
    public class AmortizationInstallmentRepository : GenericRepository<AmortizationInstallment>, IAmortizationInstallmentRepository
    {
        private readonly HermesBankingContext _context;

        public AmortizationInstallmentRepository(HermesBankingContext context) : base(context)
        {
            _context = context;
        }

        // Implementación de métodos adicionales para manejar cuotas de amortización, si es necesario
    }
}
