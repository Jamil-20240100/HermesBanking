﻿using HermesBanking.Core.Application.DTOs.SavingsAccount;

namespace HermesBanking.Core.Application.Interfaces
{
    public interface ISavingsAccountService : IGenericService<SavingsAccountDTO>
    {
        Task<List<SavingsAccountDTO>> GetAllSavingsAccountsOfClients();
        public Task<string> GenerateUniqueAccountNumberAsync();
        Task CancelAsync(int id);
        Task TransferBalanceAndCancelAsync(int accountId);
        Task<SavingsAccountDTO?> GetByAccountNumberAsync(string beneficiaryAccountNumber);
    }
}
