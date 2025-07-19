using HermesBanking.Core.Application.ViewModels.CreditCard;
using HermesBanking.Core.Application.ViewModels.SavingsAccount;
using System.ComponentModel.DataAnnotations;

namespace HermesBanking.Core.Application.ViewModels.CashAdvance
{
    public class CashAdvanceViewModel
    {
        [Required(ErrorMessage = "Debe seleccionar la tarjeta de crédito de origen.")]
        public int SourceCreditCardId { get; set; }

        [Required(ErrorMessage = "Debe seleccionar la cuenta de ahorro de destino.")]
        public int DestinationSavingsAccountId { get; set; }

        [Required(ErrorMessage = "Debe ingresar el monto del avance.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El monto debe ser mayor que cero.")]
        public decimal Amount { get; set; }

        public List<CreditCardViewModel>? AvailableCreditCards { get; set; }
        public List<SavingsAccountViewModel>? AvailableSavingsAccounts { get; set; }

        public bool HasError { get; set; }
        public string? ErrorMessage { get; set; }
    }
}