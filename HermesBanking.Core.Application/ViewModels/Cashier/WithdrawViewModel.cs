using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace HermesBanking.Core.Application.ViewModels.Cashier
{
    public class WithdrawViewModel
    {
        [Required(ErrorMessage = "Debe ingresar el número de cuenta.")]
        public string AccountNumber { get; set; } = null!;

        [Required(ErrorMessage = "Debe ingresar un monto.")]
        [Range(1, double.MaxValue, ErrorMessage = "El monto debe ser mayor que cero.")]
        public decimal Amount { get; set; }

        public List<SelectListItem>? SavingsAccounts { get; set; } = new();
    }
}
