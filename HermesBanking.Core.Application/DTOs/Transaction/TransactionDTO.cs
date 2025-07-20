namespace HermesBanking.Core.Application.DTOs.Transaction
{
    public class TransactionDTO
    {
        public int? SavingsAccountId { get; set; }      // Opcional, si aplica
        public int? CreditCardId { get; set; }          // Opcional, si aplica
        public string Type { get; set; }                 // Ej: "Express", "PagoPrestamo", etc.
        public decimal Amount { get; set; }
        public string Origin { get; set; }               // Cuenta o tarjeta origen (número)
        public string Beneficiary { get; set; }          // Cuenta, tarjeta o préstamo destino (número o código)
        public string CashierId { get; set; }            // Usuario que realizó la transacción
        public DateTime Date { get; set; }
        public string Description { get; set; }
    }
}
