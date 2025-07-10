using HermesBanking.Core.Application.DTOs.SavingsAccount;

namespace HermesBanking.Core.Application.Interfaces
{
    public interface ISavingsAccountService : IGenericService<SavingsAccountDTO>
    {
        Task<List<SavingsAccountDTO>> GetAllSavingsAccountsOfClients();
    }
}
