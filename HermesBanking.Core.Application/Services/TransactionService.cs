using HermesBanking.Core.Application.Interfaces;
using HermesBanking.Core.Domain.Entities;
using HermesBanking.Core.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using HermesBanking.Core.Domain.Common.Enums;

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

        // Método por parámetros sueltos (ya existente)
        public async Task RegisterTransactionAsync(int savingsAccountId, string type, decimal amount, string origin, string beneficiary, string? cashierId = null)
        {
            var transaction = new Transaction
            {
                SavingsAccountId = savingsAccountId,
                Type = type,
                Amount = amount,
                Origin = origin,
                Beneficiary = beneficiary,
                PerformedByCashierId = cashierId,
                Date = DateTime.Now
            };

            await _transactionRepo.AddAsync(transaction);
        }

        // Nueva sobrecarga que acepta un objeto Transaction directamente
        public async Task RegisterTransactionAsync(Transaction transaction)
        {
            transaction.Date = DateTime.Now; // Asegura que se establezca la fecha aquí si aún no se ha definido
            await _transactionRepo.AddAsync(transaction);
        }

        // Transacciones por cajero y fecha
        public async Task<List<Transaction>> GetTransactionsByCashierAndDateAsync(string cashierId, DateTime date)
        {
            return await _transactionRepo
                .GetAllQuery()
                .Where(t => t.PerformedByCashierId == cashierId && t.Date.Date == date.Date)
                .ToListAsync();
        }
    }
}
