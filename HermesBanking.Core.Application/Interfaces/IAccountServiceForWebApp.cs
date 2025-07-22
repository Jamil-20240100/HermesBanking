// HermesBanking.Core.Application.Interfaces/IAccountServiceForWebApp.cs
using HermesBanking.Core.Application.DTOs.CreditCard;
using HermesBanking.Core.Application.DTOs.Loan;
using HermesBanking.Core.Application.DTOs.SavingsAccount;
using HermesBanking.Core.Application.DTOs.User;
using HermesBanking.Core.Application.ViewModels.Loan;
using HermesBanking.Core.Domain.Entities; // Make sure this is included for Loan entity

namespace HermesBanking.Core.Application.Interfaces
{
    public interface IAccountServiceForWebApp : IBaseAccountService
    {
        Task<List<SavingsAccountDTO>> GetSavingsAccountsByUserIdAsync(string userId);
        Task<List<CreditCardDTO>> GetCreditCardsByUserIdAsync(string userId);
        Task<List<LoanDTO>> GetLoansByUserIdAsync(string userId);
        Task<LoginResponseDto> AuthenticateAsync(LoginDto loginDto);
        Task SignOutAsync();
        Task<UserDto?> GetUserById(string userId);
        Task<string?> GetUserEmailAsync(string userId);
        Task<string> GetUserFullNameAsync(string userId);
        Task<List<ClientSelectionViewModel>> GetClientDetailsForLoanAssignment(string? cedula);
        Task UpdateSavingsAccountBalance(string clientId, decimal amount);
        Task<UserDto?> GetUserByIdentificationNumber(string identificationNumber);

        // --- Methods you've already added ---
        Task<string?> GetSavingsAccountHolderFullNameAsync(string accountNumber);
        Task<string?> GetSavingsAccountHolderEmailAsync(string accountNumber);

        // --- THIS IS THE MISSING METHOD YOU NEED TO ADD ---
        Task<Loan?> GetLoanInfoAsync(string loanIdentifier);
        // --- End of missing method ---

        // These were also mentioned in my previous suggestions for AccountServiceForWebApp
        Task<IEnumerable<SavingsAccount>> GetActiveAccountsAsync(string clientId);
        Task<CreditCard?> GetCreditCardByNumberAsync(string cardNumber);
        Task<SavingsAccount?> GetSavingsAccountByNumberAsync(string accountNumber);
        Task<string> GetUserEmailByClientIdAsync(string clientId);
        Task UpdateAsync(SavingsAccount account);
    }
}