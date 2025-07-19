using HermesBanking.Core.Domain.Entities;
using HermesBanking.Core.Domain.Interfaces;
using HermesBanking.Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace HermesBanking.Infrastructure.Persistence.Repositories
{
    public class SavingsAccountRepository : GenericRepository<SavingsAccount>, ISavingsAccountRepository
    {
        private readonly HermesBankingContext _context;
        public SavingsAccountRepository(HermesBankingContext context) : base(context)
        {
            _context = context;
        }
        public async Task<SavingsAccount?> GetByAccountNumberAsync(string accountNumber)
        {
            return await _context.SavingsAccount
                .FirstOrDefaultAsync(sa => sa.AccountNumber == accountNumber);
        }

        public async Task<SavingsAccount> GetByIdAsync(int id)
        {
            return await _context.SavingsAccount.FindAsync(id);
        }

        public async Task<IEnumerable<SavingsAccount>> GetAccountsByClientIdAsync(string clientId)
        {
            return await _context.SavingsAccount
                 .Where(account => account.ClientId == clientId)
                 .ToListAsync();
        }


    }
}
