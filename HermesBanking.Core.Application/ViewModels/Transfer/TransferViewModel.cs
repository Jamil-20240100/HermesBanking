using HermesBanking.Core.Application.ViewModels.SavingsAccount;
using System.ComponentModel.DataAnnotations;

namespace HermesBanking.Core.Application.ViewModels.Transfer
{
    public class TransferViewModel
    {
        [Required(ErrorMessage = "Debe seleccionar la cuenta de origen.")]
        public int SourceAccountId { get; set; }

        [Required(ErrorMessage = "Debe seleccionar la cuenta de destino.")]
        public int DestinationAccountId { get; set; }

        [Required(ErrorMessage = "Debe ingresar el monto a transferir.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El monto debe ser mayor que cero.")]
        public decimal Amount { get; set; }
        public List<SavingsAccountViewModel>? AvailableAccounts { get; set; }
        public bool HasError { get; set; }
        public string? ErrorMessage { get; set; }
    }
}