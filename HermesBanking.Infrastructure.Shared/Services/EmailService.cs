using HermesBanking.Core.Application.DTOs.Email;
using HermesBanking.Core.Application.Interfaces;
using HermesBanking.Core.Domain.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using MailKit.Net.Smtp; // Add this using directive for SmtpClient

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

                using var smtpClient = new SmtpClient();
                // Consider adding a timeout for connect and send operations
                await smtpClient.ConnectAsync(_mailSettings.SmtpHost, _mailSettings.SmtpPort, MailKit.Security.SecureSocketOptions.StartTls);
                await smtpClient.AuthenticateAsync(_mailSettings.SmtpUser, _mailSettings.SmtpPass);
                await smtpClient.SendAsync(email);
                await smtpClient.DisconnectAsync(true);

                _logger.LogInformation("Email sent successfully to {Recipient} with subject '{Subject}'.",
                                       string.Join(", ", emailRequestDto.ToRange ?? new List<string> { emailRequestDto.To }),
                                       emailRequestDto.Subject);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An exception occurred while sending an email to {Recipient} with subject '{Subject}'.",
                                   string.Join(", ", emailRequestDto.ToRange ?? new List<string> { emailRequestDto.To }),
                                   emailRequestDto.Subject);
                // Optionally re-throw a more user-friendly exception or handle it
                throw new InvalidOperationException("Failed to send email notification.", ex);
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

        // --- Start of newly implemented methods ---

        public async Task SendTransactionInitiatorNotificationAsync(string clientEmail, decimal amount, string destinationAccountNumberLastFourDigits, DateTime transactionDate)
        {
            string subject = $"Confirmación de Transacción Saliente - HermesBanking";
            string message = $"Estimado cliente,\n\n" +
                             $"Le informamos que se ha realizado una transacción desde su cuenta.\n\n" +
                             $"Detalles de la transacción:\n" +
                             $"- Monto: {amount:C}\n" +
                             $"- Cuenta de Destino (últimos 4 dígitos): ****{destinationAccountNumberLastFourDigits}\n" +
                             $"- Fecha y Hora: {transactionDate:G}\n\n" +
                             $"Gracias por utilizar Hermes Banking.\n\n" +
                             $"Atentamente,\nEl equipo de Hermes Banking";

            await SendEmailAsync(clientEmail, subject, message);
        }

        public async Task SendTransactionReceiverNotificationAsync(string clientEmail, decimal amount, string senderAccountNumberLastFourDigits, DateTime transactionDate)
        {
            string subject = $"Confirmación de Transacción Entrante - HermesBanking";
            string message = $"Estimado cliente,\n\n" +
                             $"Le informamos que ha recibido una transacción en su cuenta.\n\n" +
                             $"Detalles de la transacción:\n" +
                             $"- Monto: {amount:C}\n" +
                             $"- Cuenta de Origen (últimos 4 dígitos): ****{senderAccountNumberLastFourDigits}\n" +
                             $"- Fecha y Hora: {transactionDate:G}\n\n" +
                             $"Gracias por utilizar Hermes Banking.\n\n" +
                             $"Atentamente,\nEl equipo de Hermes Banking";

            await SendEmailAsync(clientEmail, subject, message);
        }

        public async Task SendCreditCardPaymentNotificationAsync(string clientEmail, decimal amount, string creditCardNumberLastFourDigits, string fromAccountNumberLastFourDigits, DateTime transactionDate)
        {
            string subject = $"Confirmación de Pago a Tarjeta de Crédito - HermesBanking";
            string message = $"Estimado cliente,\n\n" +
                             $"Se ha realizado un pago a su tarjeta de crédito.\n\n" +
                             $"Detalles del pago:\n" +
                             $"- Monto Pagado: {amount:C}\n" +
                             $"- Tarjeta de Crédito (últimos 4 dígitos): ****{creditCardNumberLastFourDigits}\n" +
                             $"- Cuenta de Origen (últimos 4 dígitos): ****{fromAccountNumberLastFourDigits}\n" +
                             $"- Fecha y Hora: {transactionDate:G}\n\n" +
                             $"Gracias por utilizar Hermes Banking.\n\n" +
                             $"Atentamente,\nEl equipo de Hermes Banking";

            await SendEmailAsync(clientEmail, subject, message);
        }

        public async Task SendLoanPaymentNotificationAsync(string clientEmail, decimal amount, string loanIdentifier, string fromAccountNumberLastFourDigits, DateTime transactionDate)
        {
            string subject = $"Confirmación de Pago de Préstamo - HermesBanking";
            string message = $"Estimado cliente,\n\n" +
                             $"Se ha realizado un pago a su préstamo.\n\n" +
                             $"Detalles del pago:\n" +
                             $"- Monto Pagado: {amount:C}\n" +
                             $"- ID de Préstamo: {loanIdentifier}\n" +
                             $"- Cuenta de Origen (últimos 4 dígitos): ****{fromAccountNumberLastFourDigits}\n" +
                             $"- Fecha y Hora: {transactionDate:G}\n\n" +
                             $"Gracias por utilizar Hermes Banking.\n\n" +
                             $"Atentamente,\nEl equipo de Hermes Banking";

            await SendEmailAsync(clientEmail, subject, message);
        }

        // --- End of newly implemented methods ---

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