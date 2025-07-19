using HermesBanking.Core.Application.DTOs.Transaction;
using HermesBanking.Core.Application.Interfaces;
using HermesBanking.Core.Domain.Entities;
using HermesBanking.Core.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HermesBanking.Infrastructure.Application.Services
{
    public class TransactionService : ITransactionService
    {
        private readonly ITransactionRepository _transactionRepo;
        private readonly IAccountServiceForWebApp _userService;

        public TransactionService(ITransactionRepository transactionRepo, IAccountServiceForWebApp userService)
        {
            _transactionRepo = transactionRepo;
            _userService = userService;
        }

        public async Task<bool> RegisterTransactionAsync(TransactionDTO transactionDto)
        {
            var transaction = new Transaction
            {
                SavingsAccountId = transactionDto.SavingsAccountId,
                Type = transactionDto.Type,
                Amount = transactionDto.Amount,
                Origin = transactionDto.Origin,
                Beneficiary = transactionDto.Beneficiary,
                PerformedByCashierId = transactionDto.CashierId,
                Date = transactionDto.Date
            };

            // Guardar la transacción en el repositorio
            await _transactionRepo.AddAsync(transaction);

            return true;
        }

        public async Task<List<Transaction>> GetTransactionsByCashierAndDateAsync(string cashierId, DateTime date)
        {
            return await _transactionRepo
                .GetAllQuery()
                .Where(t => t.PerformedByCashierId == cashierId && t.Date.Date == date.Date)
                .ToListAsync();
        }
    }
}
