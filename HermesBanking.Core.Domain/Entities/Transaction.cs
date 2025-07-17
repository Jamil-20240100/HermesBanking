using HermesBanking.Core.Domain.Common.Enums;

namespace HermesBanking.Core.Domain.Entities
{
    public class Transaction
    {
        public int Id { get; set; }
        public int? SavingsAccountId { get; set; }
        public string Type { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Origin { get; set; } = string.Empty;
        public string Beneficiary { get; set; } = string.Empty;
        public string? PerformedByCashierId { get; set; }
        public DateTime Date { get; set; }

        // NUEVAS PROPIEDADES (para pagos)
        public string? Description { get; set; }
        public string? CashierId { get; set; }
        public string? ClientId { get; set; }
        public TransactionType? TransactionType { get; set; }

        public virtual SavingsAccount SavingsAccount { get; set; }
    }
}
