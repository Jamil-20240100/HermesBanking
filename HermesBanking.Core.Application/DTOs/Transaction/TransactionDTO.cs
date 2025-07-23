using HermesBanking.Core.Domain.Common.Enums;
using System;

namespace HermesBanking.Core.Application.DTOs.Transaction
{
    public class TransactionDTO
    {
        public int Id { get; set; }
        public decimal Amount { get; set; }
        public DateTime TransactionDate { get; set; }
        public TransactionType? TransactionType { get; set; }

        public string SourceAccountId { get; set; } = string.Empty;
        public string? DestinationAccountId { get; set; }
        public string? DestinationCardId { get; set; }
        public int? DestinationLoanId { get; set; }
        public string? Description { get; set; }
        public int SavingsAccountId { get; set; }
        public string? Type { get; set; }
        public string? Origin { get; set; }
        public string? Beneficiary { get; set; }
        public string? CashierId { get; set; }
        public DateTime Date { get; set; }
    }
}