using HermesBanking.Core.Application.DTOs.User;
namespace HermesBanking.Core.Application.Interfaces
{
    public interface IAccountServiceForWebApi : IBaseAccountService
    {
        Task<LoginResponseForApiDTO> AuthenticateAsync(LoginDto loginDto);

        Task<bool> ConfirmAccountAsync(string userId, string token);
        Task<UserResponseDto> ForgotPasswordWithTokenAsync(ForgotPasswordWithTokenDto request);
        Task<(bool Success, string? ErrorMessage)> CreateUserAsync(CreateUserRequestDto request);
        Task<bool> CommerceHasUserAsync(string commerceId);
        Task<UserResponseDto> CreateCommerceUserAsync(SaveUserDto dto);
        Task<List<UserDto>> GetUsersByCommerceIdAsync(string commerceId);
    }
}