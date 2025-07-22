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
        Task ExecutePayBeneficiaryTransactionAsync(PayBeneficiaryDTO dto); // <-- Este es el método a implementar
        Task PerformTransactionAsync(TransactionRequestDto request);
        Task PayCreditCardAsync(DTOs.Transaction.CreditCardPaymentDto paymentDto);
        Task PayLoanAsync(DTOs.Transaction.LoanPaymentDto paymentDto);
        Task<List<TransactionDTO>> GetAllTransactionsAsync();
    }
}