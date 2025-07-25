using HermesBanking.Core.Application.DTOs.Transaction;
using HermesBanking.Core.Domain.Entities;
using HermesBanking.Core.Application.Interfaces;
using HermesBanking.Core.Domain.Interfaces;
using HermesBanking.Core.Domain.Common.Enums;

namespace HermesBanking.Core.Application.Services
{
    public class CashierTransactionService : ICashierTransactionService
    {
        private readonly ITransactionRepository _transactionRepo;
        private readonly ITransactionService _transactionService; // Usamos el servicio general para las transacciones no relacionadas con cajeros
        private readonly ISavingsAccountRepository _accountRepo;

        public CashierTransactionService(
            ITransactionRepository transactionRepo,
            ITransactionService transactionService,
            ISavingsAccountRepository accountRepo)
        {
            _transactionRepo = transactionRepo;
            _transactionService = transactionService;
            _accountRepo = accountRepo;
        }

        // Registro de transacciones de cajero (solo las que tienen CashierId)
        public async Task<bool> RegisterCashierTransactionAsync(TransactionDTO transactionDto)
        {
            // Transacción del cajero que tiene CashierId
            var transaction = new Transaction
            {
                SavingsAccountId = transactionDto.SavingsAccountId.ToString(),
                Type = transactionDto.Type,
                TransactionType = transactionDto.TransactionType,  // Asignar el tipo de transacción
                Amount = transactionDto.Amount,
                Origin = transactionDto.Origin,
                Beneficiary = transactionDto.Beneficiary,
                Date = transactionDto.Date,  // Fecha
                TransactionDate = transactionDto.TransactionDate,  // Aquí se usa el TransactionDate
                Status = Status.APPROVED,
                Description = $"Transacción de RD$ {transactionDto.Amount} tipo {transactionDto.TransactionType}",
                CashierId = transactionDto.CashierId,
                DestinationLoanId = transactionDto.DestinationLoanId,
            };

            // Agregar la transacción a la base de datos
            await _transactionRepo.AddAsync(transaction);
            return true; // Retornar true para indicar que la transacción fue registrada correctamente
        }

        // Procesamiento de transferencias realizadas por el cajero
        public async Task<bool> ProcessCashierTransferAsync(string sourceAccountNumber, string destinationAccountNumber, decimal amount, string cashierId)
        {
            // Verificamos las cuentas de origen y destino
            var sourceAccount = await _accountRepo.GetByAccountNumberAsync(sourceAccountNumber);
            var destinationAccount = await _accountRepo.GetByAccountNumberAsync(destinationAccountNumber);

            if (sourceAccount == null || !sourceAccount.IsActive)
                throw new InvalidOperationException("La cuenta de origen no es válida o está inactiva.");
            if (destinationAccount == null || !destinationAccount.IsActive)
                throw new InvalidOperationException("La cuenta de destino no es válida o está inactiva.");
            if (sourceAccount.Balance < amount)
                throw new InvalidOperationException("Fondos insuficientes en la cuenta de origen.");

            // Realizamos la transferencia de fondos
            sourceAccount.Balance -= amount;
            destinationAccount.Balance += amount;

            // Actualizamos las cuentas
            await _accountRepo.UpdateAsync(sourceAccount.Id, sourceAccount);
            await _accountRepo.UpdateAsync(destinationAccount.Id, destinationAccount);

            // Crear transacción de débito (de la cuenta origen)
            var debitTransactionDto = new TransactionDTO
            {
                SavingsAccountId = sourceAccount.Id,
                Type = "TRANSFERENCIA",
                Amount = amount,
                Origin = sourceAccountNumber,
                Beneficiary = destinationAccountNumber,
                CashierId = cashierId,
                Date = DateTime.Now
            };

            // Crear transacción de crédito (a la cuenta destino)
            var creditTransactionDto = new TransactionDTO
            {
                SavingsAccountId = destinationAccount.Id,
                Type = "TRANSFERENCIA",
                Amount = amount,
                Origin = sourceAccountNumber,
                Beneficiary = destinationAccountNumber,
                CashierId = cashierId,
                Date = DateTime.Now
            };

            // Registramos ambas transacciones
            await RegisterCashierTransactionAsync(debitTransactionDto);
            await RegisterCashierTransactionAsync(creditTransactionDto);

            return true; // Retornar true si la transferencia fue procesada correctamente
        }
    }
}
