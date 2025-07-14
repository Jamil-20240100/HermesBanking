using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace HermesBanking.Core.Application.ViewModels.Cashier
{
    public class PagoPrestamoViewModel
    {
        [Required]
        [Display(Name = "Cuenta origen")]
        public string AccountNumber { get; set; } = null!;

        [Required]
        [Display(Name = "Monto a pagar")]
        [Range(1, double.MaxValue, ErrorMessage = "El monto debe ser mayor que 0.")]
        public decimal Amount { get; set; }

        [Required]
        [Display(Name = "Número del préstamo (9 dígitos)")]
        [StringLength(9, MinimumLength = 9, ErrorMessage = "Debe contener 9 dígitos")]
        public string LoanNumber { get; set; } = null!;

        public List<SelectListItem>? SavingsAccounts { get; set; } = new();
    }
}
