using HermesBanking.Core.Domain.Common.Enums;

namespace HermesBanking.Core.Application.DTOs.Transaction
{
    public class CreditCardTransactionDTO
    {
        public int Id { get; set; }
        public decimal Amount { get; set; }
        public DateTime TransactionDate { get; set; }
        public TransactionType TransactionType { get; set; }
        public string SourceAccountId { get; set; } = string.Empty;
        public string CreditCardId { get; set; } = string.Empty; // Represents the ID of the credit card entity
        public string? Description { get; set; }
    }
}
