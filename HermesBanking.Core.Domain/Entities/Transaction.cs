// HermesBanking.Core.Domain.Entities/Transaction.cs
// ¡Versión con todas las propiedades nullable para máxima compatibilidad y flexibilidad!

using System;
using HermesBanking.Core.Domain.Common.Enums; // Asegúrate de tener TransactionType y Status aquí

namespace HermesBanking.Core.Domain.Entities
{
    public class Transaction
    {
        public int Id { get; set; } // int siempre es non-nullable, 0 es su valor por defecto.

        // Propiedades originales:
        public string? SavingsAccountId { get; set; } // Ahora nullable
        public decimal Amount { get; set; } // decimal siempre es non-nullable, 0.0m es su valor por defecto.
        public string? Type { get; set; } // Ahora nullable (será reemplazado por TransactionType)
        public string? Origin { get; set; } // Ahora nullable (será reemplazado por SourceAccountId)
        public string? Beneficiary { get; set; } // Ahora nullable (será reemplazado por DestinationAccountId/CardId/LoanId)
        public DateTime? Date { get; set; } // Ahora nullable (será reemplazado por TransactionDate)

        // Nuevas propiedades introducidas:
        public string? CashierId { get; set; } // Ahora nullable
        public string? SourceAccountId { get; set; } // Ahora nullable
        public string? DestinationAccountId { get; set; } // Ahora nullable
        public DateTime? TransactionDate { get; set; } // Ahora nullable
        public int? DestinationLoanId { get; set; } // int? es el tipo nullable para int
        public TransactionType? TransactionType { get; set; } // Enum nullable
        public string? Description { get; set; } // Ahora nullable
        public string? DestinationCardId { get; set; } // Ahora nullable
        public Status? Status { get; set; } // Enum nullable
        public string? CreditCardId { get; set; } // int? es el tipo nullable para int

        // Constructor vacío (no necesita inicializar ya que todo es nullable o tipo de valor con default)
        public Transaction() { }

        // Si tu entidad Transaction heredaba de BaseEntity y BaseEntity ya tenía Id,
        // podrías hacer esto (pero por ahora lo dejo simple):
        // public class Transaction : BaseEntity { /* ... */ }
        // Y eliminar public int Id { get; set; }
    }
}