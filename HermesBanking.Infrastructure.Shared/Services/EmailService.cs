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
                emailRequestDto.ToRange?.Add(emailRequestDto.To ?? "");

                MimeMessage email = new()
                {
                    Sender = MailboxAddress.Parse(_mailSettings.EmailFrom),
                    Subject = emailRequestDto.Subject
                };

                foreach (var toItem in emailRequestDto.ToRange ?? [])
                {
                    email.To.Add(MailboxAddress.Parse(toItem));
                }

                BodyBuilder builder = new()
                {
                    HtmlBody = emailRequestDto.HtmlBody
                };
                email.Body = builder.ToMessageBody();

                using MailKit.Net.Smtp.SmtpClient smtpClient = new();
                await smtpClient.ConnectAsync(_mailSettings.SmtpHost, _mailSettings.SmtpPort, MailKit.Security.SecureSocketOptions.StartTls);
                await smtpClient.AuthenticateAsync(_mailSettings.SmtpUser, _mailSettings.SmtpPass);
                await smtpClient.SendAsync(email);
                await smtpClient.DisconnectAsync(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An exception occured {Exception}.", ex);
            }
        }

        // Método de envío simple para simular el envío de un correo.
        public async Task SendEmailAsync(string toEmail, string subject, string message)
        {
            Console.WriteLine($"Simulando envío de correo a: {toEmail}");
            Console.WriteLine($"Asunto: {subject}");
            Console.WriteLine($"Mensaje: {message}");
            await Task.CompletedTask;
        }

        // Método para enviar el correo cuando un préstamo ha sido aprobado.
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

        // Método para enviar un correo cuando la tasa de interés del préstamo se actualiza.
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

        // Método para enviar una confirmación de transacción.
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
