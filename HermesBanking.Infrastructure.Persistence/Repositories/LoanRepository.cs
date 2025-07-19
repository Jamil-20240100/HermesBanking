using HermesBanking.Core.Domain.Entities;
using HermesBanking.Core.Domain.Interfaces;
using HermesBanking.Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace HermesBanking.Infrastructure.Persistence.Repositories
{
    public class LoanRepository : GenericRepository<Loan>, ILoanRepository
    {
        public LoanRepository(HermesBankingContext context) : base(context)
        {
        }

        
        public async Task<Loan?> GetLoanByIdentifierAsync(string loanIdentifier)
        {
            return await _context.Loans
                .FirstOrDefaultAsync(l => l.LoanIdentifier == loanIdentifier);  // Busca el préstamo por su identificador
        }
    }
}
