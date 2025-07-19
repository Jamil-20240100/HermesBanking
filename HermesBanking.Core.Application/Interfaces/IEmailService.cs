using HermesBanking.Core.Application.DTOs.Email;

namespace HermesBanking.Core.Application.Interfaces
{
    public interface IEmailService
    {
        // Enviar correo con la información de EmailRequestDto.
        Task SendAsync(EmailRequestDto emailRequestDto);

        // Enviar correo simple, pasando directamente el correo y el mensaje.
        Task SendEmailAsync(string toEmail, string subject, string message);

        // Enviar correo cuando un préstamo ha sido aprobado.
        Task SendLoanApprovedEmail(string clientEmail, decimal amount, int term, decimal interestRate, decimal monthlyInstallment);

        // Enviar correo cuando se actualiza la tasa de interés de un préstamo.
        Task SendLoanInterestRateUpdatedEmail(string clientEmail, decimal newInterestRate, decimal newMonthlyInstallment, decimal oldMonthlyInstallment);

        // Obtener el correo electrónico de un usuario dado su ID.
        Task<string> GetUserEmailByClientId(string clientId);

        // Enviar correo de confirmación de transacción.
        Task SendTransactionConfirmationEmail(string clientEmail, decimal amount, string sourceAccount, string destinationAccount, DateTime transactionDate);
    }
}
