using HermesBanking.Core.Application.DTOs.Transaction;
using System.Threading.Tasks;

namespace HermesBanking.Core.Application.Interfaces
{
    public interface ICashierTransactionService
    {
        // Método para registrar transacciones del cajero (con CashierId)
        Task<bool> RegisterCashierTransactionAsync(TransactionDTO transactionDto);

        // Método para procesar transferencias hechas por el cajero
        Task<bool> ProcessCashierTransferAsync(string sourceAccountNumber, string destinationAccountNumber, decimal amount, string cashierId);
    }
}
