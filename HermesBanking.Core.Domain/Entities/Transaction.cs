using HermesBanking.Core.Domain.Common.Enums;

namespace HermesBanking.Core.Domain.Entities
{
    public class Transaction
    {
        public int Id { get; set; }

        // Tipo de transacción (ej. "DEPÓSITO", "RETIRO", etc.)
        public string Type { get; set; } = string.Empty;

        public decimal Amount { get; set; }

        public string Origin { get; set; } = string.Empty;

        public string Beneficiary { get; set; } = string.Empty;

        public DateTime Date { get; set; } = DateTime.Now;

        // En una versión era nullable, en otra no. Dejamos nullable por seguridad
        public int? SavingsAccountId { get; set; }

        public virtual SavingsAccount? SavingsAccount { get; set; }

        public string? PerformedByCashierId { get; set; }

        // Esto es nuevo, relacionado con estados de la transacción
        public Status? Status { get; set; }

        // Nuevo campo para pagos con tarjeta
        public int? CreditCardId { get; set; }

        // Campo Descripción (nullable por seguridad)
        public string? Description { get; set; }

        // NUEVAS propiedades específicas de contexto de caja
        public string? CashierId { get; set; }

        public string? ClientId { get; set; }

        // Enum que categoriza la transacción (mejor que un string plano en "Type")
        public TransactionType? TransactionType { get; set; }
    }
}
