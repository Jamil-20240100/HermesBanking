namespace HermesBanking.Core.Application.DTOs.Loan
{
    public class AmortizationInstallmentDTO
    {
        public int Id { get; set; }
        public int LoanId { get; set; }
        public int InstallmentNumber { get; set; }
        public DateTime PaymentDate { get; set; }
        public decimal InstallmentValue { get; set; }
        public decimal PrincipalAmount { get; set; }
        public decimal InterestAmount { get; set; }
        public decimal RemainingBalance { get; set; }
        public bool IsPaid { get; set; }
        public bool IsOverdue { get; set; }
        public DateTime? PaidDate { get; set; }
    }
}