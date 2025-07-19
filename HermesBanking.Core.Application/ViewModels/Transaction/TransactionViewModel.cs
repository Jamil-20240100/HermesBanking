using System.ComponentModel.DataAnnotations;

namespace HermesBanking.Core.Application.ViewModels
{
    public class TransactionViewModel
    {
        [Required]
        public int SavingsAccountId { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "El monto debe ser mayor que cero.")]
        public decimal Amount { get; set; }

        [Required]
        public string Type { get; set; }

        [Required]
        public string Origin { get; set; }

        [Required]
        public string Beneficiary { get; set; }

        [Required]
        public string CashierId { get; set; }

        public string ErrorMessage { get; set; }
        public bool HasError { get; set; }
        public DateTime Date { get; set; }
    }
}
