namespace HermesBanking.Core.Application.ViewModels.Loan
{
    public class LoanViewModel
    {
        public int Id { get; set; }
        public string? LoanIdentifier { get; set; }
        public string? ClientFullName { get; set; }
        public string? ClientId { get; set; }
        public string? ClientIdentificationNumber { get; set; }
        public decimal Amount { get; set; }
        public int TotalInstallments { get; set; }
        public int PaidInstallments { get; set; }
        public decimal PendingAmount { get; set; }
        public decimal InterestRate { get; set; }
        public int LoanTermMonths { get; set; }
        public string? Status { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}