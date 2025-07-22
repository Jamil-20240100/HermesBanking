// HermesBanking.Core.Application.Interfaces/ITransactionService.cs
using HermesBanking.Core.Application.DTOs;
using HermesBanking.Core.Application.DTOs.Cashier;
using HermesBanking.Core.Application.DTOs.Transaction;
using HermesBanking.Core.Application.DTOs.Transfer;
using System.Threading.Tasks;

namespace HermesBanking.Core.Application.Interfaces
{
    public interface ITransactionService
    {
        Task ExecuteExpressTransactionAsync(ExpressTransactionDTO dto);
        Task ProcessTransferAsync(TransferDTO dto);
        Task ExecutePayCreditCardTransactionAsync(PayCreditCardDTO dto);
        Task ExecutePayLoanTransactionAsync(PayLoanDTO dto);
        Task ExecutePayBeneficiaryTransactionAsync(PayBeneficiaryDTO dto); // <-- Este es el método a implementar

        Task<bool> ValidateAccountExistsAndActive(string accountNumber);
        Task<bool> HasSufficientFunds(string accountNumber, decimal amount);
        Task<bool> ValidateCreditCardExistsAndActive(string cardNumber);
        Task<bool> ValidateLoanExistsAndActive(string loanIdentifier);
        Task<bool> ValidateBeneficiaryExists(string beneficiaryAccountNumber); // <--- Podría usarse internamente

        Task<TransactionDTO?> GetByIdAsync(int id);
        Task<bool> RegisterTransactionAsync(TransactionDTO transactionDto);

        Task PerformTransactionAsync(TransactionRequestDto request);
        Task PayCreditCardAsync(DTOs.Transaction.CreditCardPaymentDto paymentDto);
        Task PayLoanAsync(DTOs.Transaction.LoanPaymentDto paymentDto);
    }
}