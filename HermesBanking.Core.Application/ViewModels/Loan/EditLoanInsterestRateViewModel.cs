using System.ComponentModel.DataAnnotations;

namespace HermesBanking.Core.Application.ViewModels.Loan
{
    public class EditLoanInterestRateViewModel
    {
        public int LoanId { get; set; }

        [Required(ErrorMessage = "La nueva tasa de interés es obligatoria.")]
        [Range(0.01, 100.00, ErrorMessage = "La tasa de interés debe ser entre 0.01 y 100.")]
        [Display(Name = "Nueva Tasa de Interés Anual (%)")]
        public decimal NewInterestRate { get; set; }
    }
}