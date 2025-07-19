using HermesBanking.Core.Application.DTOs.User;

namespace HermesBanking.Core.Application.Interfaces
{
    public interface IAccountServiceForWebApi : IBaseAccountService
    {
        Task<LoginResponseForApiDto> AuthenticateAsync(LoginDto loginDto);  // Tipo de retorno correcto
    }
}