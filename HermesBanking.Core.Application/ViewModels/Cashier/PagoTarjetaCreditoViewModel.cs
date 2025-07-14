using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace HermesBanking.Core.Application.ViewModels.Cashier
{
    public class PagoTarjetaCreditoViewModel
    {
        [Required(ErrorMessage = "Cuenta origen requerida")]
        [Display(Name = "Número de cuenta origen")]
        public string AccountNumber { get; set; } = null!;

        [Required(ErrorMessage = "Monto requerido")]
        [Range(1, double.MaxValue, ErrorMessage = "Monto inválido")]
        public decimal Amount { get; set; }

        [Required(ErrorMessage = "Número de tarjeta requerido")]
        [StringLength(16, MinimumLength = 16, ErrorMessage = "Debe tener 16 dígitos")]
        [Display(Name = "Número de tarjeta")]
        public string CardNumber { get; set; } = null!;

        public List<SelectListItem>? SavingsAccounts { get; set; } = new();
    }
}
