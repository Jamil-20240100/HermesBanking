using HermesBanking.Core.Application.DTOs.Email;
using HermesBanking.Core.Application.Interfaces;
using HermesBanking.Core.Domain.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;

namespace HermesBanking.Infrastructure.Shared.Services
{
    public class EmailService : IEmailService
    {
        private readonly MailSettings _mailSettings;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IOptions<MailSettings> mailSettings, ILogger<EmailService> logger)
        {
            _mailSettings = mailSettings.Value;
            _logger = logger;
        }

        public async Task SendAsync(EmailRequestDto emailRequestDto)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(emailRequestDto.To))
                {
                    emailRequestDto.ToRange ??= new List<string>();
                    if (!emailRequestDto.ToRange.Contains(emailRequestDto.To))
                        emailRequestDto.ToRange.Add(emailRequestDto.To);
                }

                var email = new MimeMessage
                {
                    Sender = MailboxAddress.Parse(_mailSettings.EmailFrom),
                    Subject = emailRequestDto.Subject
                };

                foreach (var toItem in emailRequestDto.ToRange ?? Enumerable.Empty<string>())
                {
                    email.To.Add(MailboxAddress.Parse(toItem));
                }

                var builder = new BodyBuilder
                {
                    HtmlBody = emailRequestDto.HtmlBody
                };

                email.Body = builder.ToMessageBody();

                using var smtpClient = new MailKit.Net.Smtp.SmtpClient();
                await smtpClient.ConnectAsync(_mailSettings.SmtpHost, _mailSettings.SmtpPort, MailKit.Security.SecureSocketOptions.StartTls);
                await smtpClient.AuthenticateAsync(_mailSettings.SmtpUser, _mailSettings.SmtpPass);
                await smtpClient.SendAsync(email);
                await smtpClient.DisconnectAsync(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An exception occurred while sending an email.");
            }
        }

        public async Task SendEmailAsync(string toEmail, string subject, string message)
        {
            var emailRequest = new EmailRequestDto
            {
                To = toEmail,
                Subject = subject,
                HtmlBody = $"<p>{message.Replace("\n", "<br>")}</p>"
            };

            await SendAsync(emailRequest);
        }

        public async Task SendLoanApprovedEmail(string clientEmail, decimal amount, int term, decimal interestRate, decimal monthlyInstallment)
        {
            string subject = "¡Préstamo Aprobado - Hermes Banking!";
            string message = $"Estimado cliente,\n\n" +
                             $"Nos complace informarle que su solicitud de préstamo ha sido aprobada.\n\n" +
                             $"Detalles del préstamo:\n" +
                             $"- Monto Aprobado: {amount:C}\n" +
                             $"- Plazo del Préstamo: {term} meses\n" +
                             $"- Tasa de Interés Anual: {interestRate:F2}%\n" +
                             $"- Cuota Mensual: {monthlyInstallment:C}\n\n" +
                             $"Gracias por confiar en Hermes Banking.\n\n" +
                             $"Atentamente,\nEl equipo de Hermes Banking";

            await SendEmailAsync(clientEmail, subject, message);
        }

        public async Task SendLoanInterestRateUpdatedEmail(string clientEmail, decimal newInterestRate, decimal newMonthlyInstallment, decimal oldMonthlyInstallment)
        {
            string subject = "Actualización de Tasa de Interés de su Préstamo - Hermes Banking";
            string message = $"Estimado cliente,\n\n" +
                             $"Le informamos que la tasa de interés de su préstamo ha sido actualizada.\n\n" +
                             $"Detalles de la actualización:\n" +
                             $"- Antigua Cuota Mensual: {oldMonthlyInstallment:C}\n" +
                             $"- Nueva Tasa de Interés Anual: {newInterestRate:F2}%\n" +
                             $"- Nueva Cuota Mensual a partir de la próxima fecha de vencimiento: {newMonthlyInstallment:C}\n\n" +
                             $"Si tiene alguna pregunta, no dude en contactarnos.\n\n" +
                             $"Atentamente,\nEl equipo de Hermes Banking";

            await SendEmailAsync(clientEmail, subject, message);
        }

        public async Task SendTransactionConfirmationEmail(string clientEmail, decimal amount, string sourceAccount, string destinationAccount, DateTime transactionDate)
        {
            string subject = $"Confirmación de Transacción: {amount:C} de {sourceAccount} a {destinationAccount}";
            string message = $"Estimado cliente,\n\n" +
                             $"Su transacción ha sido completada con éxito.\n\n" +
                             $"Detalles de la transacción:\n" +
                             $"- Monto: {amount:C}\n" +
                             $"- Cuenta de Origen: {sourceAccount}\n" +
                             $"- Cuenta de Destino: {destinationAccount}\n" +
                             $"- Fecha y Hora: {transactionDate:G}\n\n" +
                             $"Gracias por utilizar Hermes Banking.\n\n" +
                             $"Atentamente,\nEl equipo de Hermes Banking";

            await SendEmailAsync(clientEmail, subject, message);
        }
    }
}
