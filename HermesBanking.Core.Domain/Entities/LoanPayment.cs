namespace HermesBanking.Core.Domain.Entities
{
    public class LoanPayment
    {
        public int Id { get; set; }
        public int SavingsAccountId { get; set; }
        public int LoanId { get; set; }
        public decimal Amount { get; set; }
        public DateTime PaymentDate { get; set; }

        public virtual SavingsAccount SavingsAccount { get; set; }
        public virtual Loan Loan { get; set; }
    }
}
