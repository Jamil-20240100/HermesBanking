using System.ComponentModel.DataAnnotations;

namespace HermesBanking.Core.Application.ViewModels.Loan
{
    public class AssignLoanViewModel
    {
        public string? ClientId { get; set; }
        public string? ClientFullName { get; set; }

        [Required(ErrorMessage = "El monto a prestar es obligatorio.")]
        [Range(0.01, (double)decimal.MaxValue, ErrorMessage = "El monto debe ser mayor que cero.")]
        [DataType(DataType.Currency)]
        public decimal Amount { get; set; }

        [Required(ErrorMessage = "El plazo del préstamo es obligatorio.")]
        [Range(6, 60, ErrorMessage = "El plazo debe estar entre 6 y 60 meses.")]
        public int LoanTermMonths { get; set; }

        [Required(ErrorMessage = "La tasa de interés es obligatoria.")]
        [Range(0.01, 100.00, ErrorMessage = "La tasa de interés debe ser entre 0.01 y 100.")]
        [Display(Name = "Tasa de Interés Anual (%)")]
        public decimal InterestRate { get; set; }
        public string? CreatedByAdminId { get; set; }
        public string? AdminFullName { get; set; }
    }
}