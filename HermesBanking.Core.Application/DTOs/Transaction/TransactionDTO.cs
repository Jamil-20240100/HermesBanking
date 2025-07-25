// HermesBanking.Core.Application.DTOs.Transaction/TransactionDTO.cs (General DTO for display)
using HermesBanking.Core.Domain.Common.Enums;
using HermesBanking.Core.Application.DTOs.Commerce;
using System;

namespace HermesBanking.Core.Application.DTOs.Transaction
{
    public class TransactionDTO
    {
        public int Id { get; set; } // Must be present for mapping from entity
        public decimal Amount { get; set; }
        public DateTime TransactionDate { get; set; } // Matches entity
        public TransactionType? TransactionType { get; set; } // Matches entity
        public CommerceDTO? Commerce { get; set; }

        public string SourceAccountId { get; set; } = string.Empty;
        public string? DestinationAccountId { get; set; }
        public string? DestinationCardId { get; set; }
        public string? CreditCardId { get; set; }
        public int? DestinationLoanId { get; set; }
        public string? Description { get; set; } // Matches entity
        public int SavingsAccountId { get;set; }
        public string Type { get; set; }
        public string Origin { get;  set; }
        public string Beneficiary { get;  set; }
        public string CashierId { get;  set; }
        public DateTime Date { get;  set; }
        public Status? Status { get; set; }
    }
}

// HermesBanking.Core.Application.DTOs.Transaction/ExpressTransactionDTO.cs
// This DTO seems to be an input DTO for an express transaction (like a simple transfer)