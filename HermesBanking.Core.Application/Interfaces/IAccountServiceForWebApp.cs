using HermesBanking.Core.Application.DTOs.User;

namespace HermesBanking.Core.Application.Interfaces
{
    public interface IAccountServiceForWebApp : IBaseAccountService
    {
        Task<LoginResponseDto> AuthenticateAsync(LoginDto loginDto); 
        Task SignOutAsync();
        Task<UserDto?> GetUserByIdAsync(string userId);
        Task<string?> GetUserEmailAsync(string userId);
        Task<string> GetUserFullNameAsync(string userId);
    }
}