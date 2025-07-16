namespace HermesBanking.Core.Application.DTOs.Loan
{
    public class UpdateLoanInterestRateDTO
    {
        public int LoanId { get; set; }
        public decimal NewInterestRate { get; set; }
    }
}