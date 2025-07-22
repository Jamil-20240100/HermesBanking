using HermesBanking.Core.Domain.Common.Enums;

namespace HermesBanking.Core.Application.DTOs.Transaction
{
    public class BeneficiaryTransactionDTO
    {
        public int Id { get; set; }
        public decimal Amount { get; set; }
        public DateTime TransactionDate { get; set; }
        public TransactionType TransactionType { get; set; }
        public string SourceAccountId { get; set; } = string.Empty;
        public string DestinationAccountId { get; set; } = string.Empty; // Beneficiary's account
        public string? Description { get; set; }
    }
}