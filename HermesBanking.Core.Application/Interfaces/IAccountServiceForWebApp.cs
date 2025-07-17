using HermesBanking.Core.Application.DTOs.User;
using HermesBanking.Core.Application.ViewModels.Loan;

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
        Task UpdateSavingsAccountBalance(string clientId, decimal amount);
        Task<UserDto?> GetUserByIdentificationNumber(string identificationNumber);
        Task<string> GetUserEmailByClientIdAsync(string clientId);

    }
}