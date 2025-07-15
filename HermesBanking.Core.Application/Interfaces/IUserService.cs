using HermesBanking.Core.Application.DTOs.User;

namespace HermesBanking.Core.Application.Interfaces
{
    public interface IUserService
    {
        Task<UserDto?> GetUserByIdAsync(string userId);
        Task<string?> GetUserEmailAsync(string userId);
        Task<string> GetUserFullNameAsync(string userId);
    }
}
