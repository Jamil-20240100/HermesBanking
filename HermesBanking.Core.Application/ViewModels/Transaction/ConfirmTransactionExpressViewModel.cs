using HermesBanking.Core.Application.ViewModels.SavingsAccount;
using System.ComponentModel.DataAnnotations;

namespace HermesBanking.Core.Application.ViewModels
{
    public class ConfirmTransactionExpressViewModel
    {
        [Required]
        public string SenderAccountNumber { get; set; }

        [Required]
        public string ReceiverAccountNumber { get; set; }

        [Required]
        [Range(1, double.MaxValue, ErrorMessage = "El monto debe ser mayor que cero.")]
        public decimal Amount { get; set; }

        [Required]
        public string Type { get; set; }

        [Required]
        public string TransactionType { get; set; }
    }
}
