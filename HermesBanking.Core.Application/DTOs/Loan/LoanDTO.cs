namespace HermesBanking.Core.Application.DTOs.Loan
{
    public class LoanDTO
    {
        public int Id { get; set; }
        public string? LoanIdentifier { get; set; }
        public string? ClientId { get; set; }
        public string? ClientFullName { get; set; }
        public decimal Amount { get; set; }
        public decimal InterestRate { get; set; }
        public int LoanTermMonths { get; set; }
        public decimal MonthlyInstallmentValue { get; set; }
        public int TotalInstallments { get; set; }
        public int PaidInstallments { get; set; }
        public decimal PendingAmount { get; set; }
        public bool IsActive { get; set; }
        public bool IsOverdue { get; set; }
        public string? AssignedByAdminId { get; set; }
        public string? AdminFullName { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public List<AmortizationInstallmentDTO>? AmortizationInstallments { get; set; }
    }
}