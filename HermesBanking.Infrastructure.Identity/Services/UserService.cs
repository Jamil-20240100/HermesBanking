using HermesBanking.Core.Application.DTOs.User;
using HermesBanking.Core.Application.Interfaces;
using HermesBanking.Infrastructure.Identity.Entities;
using Microsoft.AspNetCore.Identity;

namespace HermesBanking.Application.Services
{
    public class UserService : IUserService
    {
        private readonly UserManager<AppUser> _userManager;

        public UserService(UserManager<AppUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<UserDto?> GetUserByIdAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null) return null;

            var roles = await _userManager.GetRolesAsync(user);

            return new UserDto
            {
                Id = user.Id,
                UserId = user.Id,
                UserName = user.UserName ?? "",
                Name = user.Name ?? "",
                LastName = user.LastName ?? "",
                Email = user.Email ?? "",
                Role = roles.FirstOrDefault() ?? "Unknown",
                IsActive = user.IsActive,
                isVerified = user.EmailConfirmed,
                InitialAmount = null
            };
        }

        public async Task<string?> GetUserEmailAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            return user?.Email;
        }

        public async Task<string> GetUserFullNameAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            return user == null ? "" : $"{user.Name} {user.LastName}";
        }
    }
}
