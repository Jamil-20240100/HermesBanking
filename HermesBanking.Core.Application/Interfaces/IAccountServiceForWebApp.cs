using HermesBanking.Core.Application.DTOs.User;
using HermesBanking.Core.Application.ViewModels.Loan;
using HermesBanking.Core.Domain.Entities;

namespace HermesBanking.Core.Application.Interfaces
{
    
    public interface IAccountServiceForWebApp : IBaseAccountService
    {
        Task<LoginResponseDto> AuthenticateAsync(LoginDto loginDto);
        Task SignOutAsync();
        Task<UserDto?> GetUserById(string userId);
        Task<string?> GetUserEmailAsync(string userId);
        Task<string> GetUserFullNameAsync(string userId);
        Task<List<ClientSelectionViewModel>> GetClientDetailsForLoanAssignment(string? cedula);
        Task UpdateSavingsAccountBalance(string clientId, decimal amount); // Este método ya está definido
        Task<UserDto?> GetUserByIdentificationNumber(string identificationNumber);
        Task<string> GetUserEmailByClientIdAsync(string clientId);
        Task<SavingsAccount?> GetAccountByNumberAsync(string accountNumber);
        Task UpdateAsync(SavingsAccount account); // Este método ya está definido
        Task<IEnumerable<SavingsAccount>> GetActiveAccountsAsync(string clientId);
        Task<CreditCard?> GetCreditCardByNumberAsync(string cardNumber);
        Task<Loan?> GetLoanInfoAsync(string loanIdentifier);
        Task<SavingsAccount?> GetSavingsAccountByNumberAsync(string accountNumber);

        Task<List<SavingsAccount>> GetAllActiveAccounts(string clientId);
    }

}