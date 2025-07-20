namespace HermesBanking.Core.Application.ViewModels.Loan
{
    public class LoanDetailsViewModel
    {
        public int Id { get; set; }
        public string LoanIdentifier { get; set; } = string.Empty;
        public string ClientId { get; set; } = string.Empty;
        public string ClientFullName { get; set; } = string.Empty;
        public string? ClientIdentificationNumber { get; set; }
        public decimal Amount { get; set; }
        public decimal InterestRate { get; set; }
        public int LoanTermMonths { get; set; }
        public decimal MonthlyInstallmentValue { get; set; }
        public int TotalInstallments { get; set; }
        public int PaidInstallments { get; set; }
        public decimal PendingAmount { get; set; }
        public bool IsActive { get; set; }
        public bool IsOverdue { get; set; }
        public string AssignedByAdminId { get; set; } = string.Empty;
        public string AdminFullName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? CompletedAt { get; set; }

        // Colección para la tabla de amortización
        public ICollection<AmortizationInstallmentViewModel> AmortizationSchedule { get; set; } = new List<AmortizationInstallmentViewModel>();
    }
}