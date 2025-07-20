using HermesBanking.Core.Application.DTOs;
using HermesBanking.Core.Application.DTOs.Transaction;
using HermesBanking.Core.Domain.Entities;
using HermesBanking.Core.Application.DTOs.Transfer;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HermesBanking.Core.Application.Interfaces
{
    public interface ITransactionService
    {
        // Validaciones
        Task<bool> ValidateAccountExistsAndActive(string accountNumber);
        Task<bool> HasSufficientFunds(string accountNumber, decimal amount);

        // Ejecución de transacciones específicas
        Task ExecuteExpressTransactionAsync(ExpressTransactionDTO dto); // Transferencia Express
        Task ExecutePayLoanTransactionAsync(PayLoanDTO dto); // Pago de préstamo
        Task ExecutePayCreditCardTransactionAsync(PayCreditCardDTO dto); // Pago de tarjeta de crédito
        Task ExecutePayBeneficiaryTransactionAsync(PayBeneficiaryDTO dto); // Pago a beneficiario

        // Registro genérico de transacción
        Task<bool> RegisterTransactionAsync(TransactionDTO transactionDto);

        // Transferencia entre cuentas
        Task ProcessTransferAsync(TransferDTO dto); // Transferencia general entre cuentas

        Task<TransactionDTO?> GetByIdAsync(int id);
    }
}
