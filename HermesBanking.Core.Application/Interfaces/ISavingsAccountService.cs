using HermesBanking.Core.Application.DTOs.SavingsAccount;
using HermesBanking.Core.Application.DTOs.Transaction;
using HermesBanking.Core.Domain.Entities;

namespace HermesBanking.Core.Application.Interfaces
{
    public interface ISavingsAccountService : IGenericService<SavingsAccountDTO>
    {
        Task<List<SavingsAccountDTO>> GetAllSavingsAccountsOfClients();
        public Task<string> GenerateUniqueAccountNumberAsync();
        Task CancelAsync(int id);
        Task TransferBalanceAndCancelAsync(int accountId);
        Task<SavingsAccountDTO?> GetByAccountNumberAsync(string beneficiaryAccountNumber);
        Task<List<DisplayTransactionDTO>> GetSavingAccountTransactionsAsync(string savingAccountId);


    }
}
