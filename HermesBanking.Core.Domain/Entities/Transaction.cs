using HermesBanking.Core.Domain.Common.Enums;

namespace HermesBanking.Core.Domain.Entities
{
    public class Transaction
    {
        public int Id { get; set; }
        public string Type { get; set; }
        public string Origin { get; set; } = null!;
        public string Beneficiary { get; set; }
        public decimal Amount { get; set; }
        public DateTime Date { get; set; } = DateTime.Now;
        public int SavingsAccountId { get; set; }
        public SavingsAccount SavingsAccount { get; set; } = null!;

        public Status Status { get; set; }
        public int CreditCardId { get; set; }

        public string Description { get; set; }

        public string? CashierId { get; set; }
    }
}
