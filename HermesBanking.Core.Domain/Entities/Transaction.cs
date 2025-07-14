namespace HermesBanking.Core.Domain.Entities
{
    public class Transaction
    {
        public int Id { get; set; }

        public string Type { get; set; } // credito o debito

        public string Origin { get; set; } = null!; // “DEPOSITO”, cuenta origen, etc.

        public string Beneficiary { get; set; } = null!; // cuenta destino o RETIRO

        public decimal Amount { get; set; }

        public DateTime Date { get; set; } = DateTime.Now;

        public int SavingsAccountId { get; set; }

        public SavingsAccount SavingsAccount { get; set; } = null!;
        public string? PerformedByCashierId { get; set; } // FK al cajero

    }
}
