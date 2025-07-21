public class CreditCardTransactionDTO
{
    public int TransactionId { get; set; }
    public string Type { get; set; } // Tipo de transacción
    public decimal Amount { get; set; } // Monto de la transacción
    public DateTime Date { get; set; } // Fecha de la transacción
    public string Origin { get; set; } // Origen de la transacción
    public string Beneficiary { get; set; } // Beneficiario de la transacción
}