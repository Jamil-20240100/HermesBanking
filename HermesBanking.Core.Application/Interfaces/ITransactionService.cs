using HermesBanking.Core.Domain.Entities;


namespace HermesBanking.Core.Application.Interfaces
{
    public interface ITransactionService
    {
        Task RegisterTransactionAsync(int savingsAccountId, string type, decimal amount, string origin, string beneficiary, string? cashierId = null);
        Task<List<Transaction>> GetTransactionsByCashierAndDateAsync(string cashierId, DateTime date);

    }
}
