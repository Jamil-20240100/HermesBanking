using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace HermesBanking.Core.Application.ViewModels.Cashier
{
    public class DepositViewModel
    {
        [Required(ErrorMessage = "El número de cuenta es obligatorio.")]
        [Display(Name = "Número de Cuenta")]
        public string? AccountNumber { get; set; }

        [Required(ErrorMessage = "El monto es obligatorio.")]
        [Range(1, double.MaxValue, ErrorMessage = "El monto debe ser mayor que cero.")]
        [Display(Name = "Monto")]
        public decimal Amount { get; set; }

        public List<SelectListItem>? SavingsAccount { get; set; } = new();
    }
}
