using HermesBanking.Core.Application.DTOs.Email;

namespace HermesBanking.Core.Application.Interfaces
{
    public interface IEmailService
    {
        Task SendAsync(EmailRequestDto emailRequestDto);
    }
}