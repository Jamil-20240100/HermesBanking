using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace HermesBanking.Core.Application.ViewModels.Cashier
{
    public class ThirdPartyTransferViewModel
    {
        [Required(ErrorMessage = "Debe ingresar la cuenta origen.")]
        public string SourceAccountNumber { get; set; } = null!;

        [Required(ErrorMessage = "Debe ingresar la cuenta destino.")]
        public string DestinationAccountNumber { get; set; } = null!;

        [Required(ErrorMessage = "Debe ingresar un monto.")]
        [Range(1, double.MaxValue, ErrorMessage = "El monto debe ser mayor que cero.")]
        public decimal Amount { get; set; }

        public List<SelectListItem> SourceAccounts { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> DestinationAccounts { get; set; } = new List<SelectListItem>();
    }
}
