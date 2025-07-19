using HermesBanking.Core.Application.DTOs.Transfer;
using HermesBanking.Core.Application.Interfaces;
using HermesBanking.Core.Application.ViewModels.Transfer;
using HermesBanking.Core.Domain.Common.Enums;
using HermesBanking.Core.Domain.Entities;
using HermesBanking.Core.Domain.Interfaces;
using System;
using System.Threading.Tasks;

namespace HermesBanking.Core.Application.Services
{
    public class TransferService : ITransferService
    {
        private readonly ISavingsAccountRepository _savingsAccountRepo;
        private readonly ITransactionRepository _transactionRepo; // Asumiendo que existe

        public TransferService(ISavingsAccountRepository savingsAccountRepo, ITransactionRepository transactionRepo)
        {
            _savingsAccountRepo = savingsAccountRepo;
            _transactionRepo = transactionRepo;
        }

        public async Task<TransferViewModel> ProcessTransferAsync(TransferDTO dto)
        {
            var response = new TransferViewModel { HasError = false };

            var sourceAccount = await _savingsAccountRepo.GetById(dto.SourceAccountId);
            var destinationAccount = await _savingsAccountRepo.GetById(dto.DestinationAccountId);

            if (sourceAccount.Balance < dto.Amount)
            {
                response.HasError = true;
                response.ErrorMessage = "La cuenta de origen no tiene fondos suficientes para realizar esta transferencia.";
                return response;
            }

            sourceAccount.Balance -= dto.Amount;
            destinationAccount.Balance += dto.Amount;

            var debitTransaction = new Transaction
            {
                SavingsAccountId = sourceAccount.Id,
                Amount = dto.Amount,
                Type = TransactionType.DEBITO.ToString(),
                Description = $"Transferencia a cuenta {destinationAccount.AccountNumber}",
                Origin = sourceAccount.AccountNumber,
                Beneficiary = destinationAccount.AccountNumber,
                Date = DateTime.Now,
                Status = Status.APPROVED,
            };
            await _transactionRepo.AddAsync(debitTransaction);

            var creditTransaction = new Transaction
            {
                SavingsAccountId = destinationAccount.Id,
                Amount = dto.Amount,
                Type = TransactionType.CREDITO.ToString(),
                Description = $"Transferencia desde cuenta {sourceAccount.AccountNumber}",
                Origin = sourceAccount.AccountNumber,
                Beneficiary = destinationAccount.AccountNumber,
                Date = DateTime.Now,
                Status = Status.APPROVED,
            };
            await _transactionRepo.AddAsync(creditTransaction);

            await _savingsAccountRepo.UpdateAsync(sourceAccount.Id, sourceAccount);
            await _savingsAccountRepo.UpdateAsync(destinationAccount.Id, destinationAccount);

            return response;
        }
    }
}