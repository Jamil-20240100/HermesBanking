using HermesBanking.Core.Application.DTOs.Email;

namespace HermesBanking.Core.Application.Interfaces
{
    public interface IEmailService
    {
        Task SendAsync(EmailRequestDto emailRequestDto);
        Task SendEmailAsync(string toEmail, string subject, string message);
        Task SendLoanApprovedEmail(string clientEmail, decimal amount, int term, decimal interestRate, decimal monthlyInstallment);
        Task SendLoanInterestRateUpdatedEmail(string clientEmail, decimal newInterestRate, decimal newMonthlyInstallment, decimal oldMonthlyInstallment);

        //
        //
        //
        Task SendTransactionInitiatorNotificationAsync(string clientEmail, decimal amount, string destinationAccountNumberLastFourDigits, DateTime transactionDate);
        Task SendTransactionReceiverNotificationAsync(string clientEmail, decimal amount, string senderAccountNumberLastFourDigits, DateTime transactionDate);
        Task SendCreditCardPaymentNotificationAsync(string clientEmail, decimal amount, string creditCardNumberLastFourDigits, string fromAccountNumberLastFourDigits, DateTime transactionDate);
        Task SendLoanPaymentNotificationAsync(string clientEmail, decimal amount, string loanIdentifier, string fromAccountNumberLastFourDigits, DateTime transactionDate);


        Task SendCreditCardPaymentNotificationToCommerceAsync(string commerceEmail, decimal amount, string creditCardNumberLastFourDigits, string toAccountNumberLastFourDigits, DateTime transactionDate);
    }
}