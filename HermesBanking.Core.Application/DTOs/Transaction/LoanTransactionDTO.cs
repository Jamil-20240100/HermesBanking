using HermesBanking.Core.Domain.Common.Enums;

namespace HermesBanking.Core.Application.DTOs.Transaction
{
    public class LoanTransactionDTO
    {
        public int Id { get; set; }
        public decimal Amount { get; set; }
        public DateTime TransactionDate { get; set; }
        public TransactionType TransactionType { get; set; }
        public string SourceAccountId { get; set; } = string.Empty;
        public int LoanId { get; set; } // Represents the ID of the loan entity
        public string? Description { get; set; }
    }
}