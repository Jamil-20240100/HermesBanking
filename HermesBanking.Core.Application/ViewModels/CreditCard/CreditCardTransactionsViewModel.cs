using HermesBanking.Core.Domain.Entities; // Esto es para las clases Transaction y CreditCard
using TransactionNamespace = HermesBanking.Core.Domain.Entities.Transaction;
using CreditCardNamespace = HermesBanking.Core.Domain.Entities.CreditCard;

namespace HermesBanking.Core.Application.ViewModels.CreditCard
{
    public class CreditCardTransactionsViewModel
    {
        public string CardId { get; set; } // ID de la tarjeta de crédito
        public string ClientFullName { get; set; } // Nombre completo del cliente
        public decimal CreditLimit { get; set; } // Límite de crédito de la tarjeta
        public decimal TotalOwedAmount { get; set; } // Total adeudado en la tarjeta
        public DateTime ExpirationDate { get; set; } 
        public bool IsActive { get; set; }

        public List<CreditCardTransactionDTO> Transactions { get; set; } // Lista de transacciones de la tarjeta
    }

}
