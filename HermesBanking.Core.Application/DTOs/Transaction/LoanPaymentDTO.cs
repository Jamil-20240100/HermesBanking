namespace HermesBanking.Core.Application.DTOs
{
    public class LoanPaymentDTO
    {
        public int SavingsAccountId { get; set; }
        public int LoanId { get; set; }
        public decimal Amount { get; set; }
        public string ClientId { get; set; }
    }
}
