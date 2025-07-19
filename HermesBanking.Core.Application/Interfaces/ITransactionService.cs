using HermesBanking.Core.Domain.Entities;
using HermesBanking.Core.Application.DTOs.Transaction;


namespace HermesBanking.Core.Application.Interfaces
{
    public interface ITransactionService
    {
        Task<bool> RegisterTransactionAsync(TransactionDTO transactionDto);
        Task<List<Transaction>> GetTransactionsByCashierAndDateAsync(string cashierId, DateTime date);


    }
}