using HermesBanking.Core.Domain.Entities;

namespace HermesBanking.Core.Domain.Interfaces
{
    public interface ILoanRepository : IGenericRepository<Loan>
    {
        Task<Loan?> GetLoanByIdentifierAsync(string loanIdentifier);
    }
}
