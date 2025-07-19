namespace HermesBanking.Core.Application.DTOs.Loan
{
    public class CreateLoanDTO
    {
        public string? ClientId { get; set; }
        public decimal Amount { get; set; }
        public int LoanTermMonths { get; set; }
        public decimal InterestRate { get; set; }
        public string? AssignedByAdminId { get; set; }
        public string? AdminFullName { get; set; }
    }
}