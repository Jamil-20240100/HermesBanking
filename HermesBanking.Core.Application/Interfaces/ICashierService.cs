using HermesBanking.Core.Application.DTOs.Loan;
using HermesBanking.Core.Application.ViewModels.Cashier;
using HermesBanking.Core.Domain.Entities;

namespace HermesBanking.Core.Application.Interfaces
{
    public interface ICashierService
    {
        Task<bool> MakeDepositAsync(string accountNumber, decimal amount, string cashierId);
        Task<bool> MakeWithdrawAsync(string accountNumber, decimal amount, string cashierId);
        Task<bool> MakeThirdPartyTransferAsync(string sourceAccountNumber, string destinationAccountNumber, decimal amount, string cashierId);

        Task<bool> MakeCreditCardPaymentAsync(string accountNumber, string cardNumber, decimal amount, string cashierId);
        Task<bool> MakeLoanPaymentAsync(string loanIdentifier, string accountNumber, decimal amount, string cashierId);


        SavingsAccount? GetSavingsAccountByNumber(string accountNumber);
        Task<(SavingsAccount? account, CreditCard? card, string? clientFullName)> GetAccountCardAndClientNameAsync(string accountNumber, string cardNumber);
        Task<(SavingsAccount? account, string? clientFullName)> GetAccountWithClientNameAsync(string accountNumber);
        Task<(LoanDTO? loan, string clientFullName, decimal remainingDebt)> GetLoanInfoAsync(string loanIdentifier);
        Task<CashierDashboardViewModel> GetTodaySummaryAsync(string cashierId);
       


        List<SavingsAccount> GetAllActiveAccounts();
    }
}
