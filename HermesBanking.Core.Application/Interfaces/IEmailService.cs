using HermesBanking.Core.Application.DTOs.Email;

namespace HermesBanking.Core.Application.Interfaces
{
    public interface IEmailService
    {
        Task SendAsync(EmailRequestDto emailRequestDto);
        Task SendEmailAsync(string toEmail, string subject, string message);
        Task SendLoanApprovedEmail(string clientEmail, decimal amount, int term, decimal interestRate, decimal monthlyInstallment);
        Task SendLoanInterestRateUpdatedEmail(string clientEmail, decimal newInterestRate, decimal newMonthlyInstallment, decimal oldMonthlyInstallment);
    }
}