// HermesBanking.Core.Application.ViewModels.Loan/AmortizationInstallmentViewModel.cs
namespace HermesBanking.Core.Application.ViewModels.Loan
{
    public class AmortizationInstallmentViewModel
    {
        public int Id { get; set; }
        public int InstallmentNumber { get; set; }
        public DateTime DueDate { get; set; } // Make sure this is correctly populated
        public decimal InstallmentValue { get; set; }
        public decimal PrincipalAmount { get; set; }
        public decimal InterestAmount { get; set; }
        public decimal RemainingBalance { get; set; }
        public decimal AmountPaid { get; set; } // Ensure this is present
        public bool IsPaid { get; set; }
        public bool IsOverdue { get; set; }
        public DateTime? PaidDate { get; set; } // <-- CRITICAL: Make this nullable!
    }
}