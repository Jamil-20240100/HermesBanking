using System.ComponentModel.DataAnnotations;

namespace HermesBanking.Core.Application.ViewModels
{
    public class ConfirmTransactionCreditCardViewModel
    {
        [Required]
        public string CreditCardId { get; set; }

        [Required]
        public string FromAccountId { get; set; }

        [Required]
        [Range(1, double.MaxValue, ErrorMessage = "El monto debe ser mayor que cero.")]
        public decimal Amount { get; set; }

        [Required]
        public string Type { get; set; }

        [Required]
        public string TransactionType { get; set; }
    }
}
