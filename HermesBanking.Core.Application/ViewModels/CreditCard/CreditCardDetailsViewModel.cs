using HermesBanking.Core.Application.DTOs.CreditCard;
using HermesBanking.Core.Application.DTOs.Transaction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HermesBanking.Core.Application.ViewModels.CreditCard
{
    public class CreditCardDetailsViewModel
    {
        // Información de la tarjeta de crédito
        public int CreditCardId { get; set; }
        public string CardId { get; set; } // El número de la tarjeta
        public string Commerce { get; set; }
        public string ClientFullName { get; set; } // Nombre completo del cliente
        public decimal CreditLimit { get; set; } // Límite de crédito
        public decimal TotalOwedAmount { get; set; } // Monto total adeudado
        public string ExpirationDate { get; set; } // Fecha de expiración (MM/AA)


        

        // Lista de transacciones asociadas a la tarjeta de crédito
        public IEnumerable<TransactionDTO> Transactions { get; set; } // Asegúrate de mapear las transacciones a DTOs
        public CreditCardDTO CreditCard { get; set; }


        // Métodos adicionales si es necesario

        public PaginationDTO Pagination { get; set; }
    }
}
