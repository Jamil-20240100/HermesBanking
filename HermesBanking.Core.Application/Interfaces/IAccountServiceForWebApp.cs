using HermesBanking.Core.Application.DTOs.User;

namespace HermesBanking.Core.Application.Interfaces
{
    public interface IAccountServiceForWebApp : IBaseAccountService
    {
        Task<LoginResponseDto> AuthenticateAsync(LoginDto loginDto); 
        Task SignOutAsync();
    }
}