namespace HermesBanking.Core.Domain.Entities
{
    public class LoanInstallment
    {
        public int Id { get; set; }

        public int LoanId { get; set; }

        public int Number { get; set; }

        public decimal Amount { get; set; }

        public decimal AmountPaid { get; set; }

        public DateTime DueDate { get; set; }

        public Loan Loan { get; set; } = null!;

        public bool IsPaid => AmountPaid >= Amount;
    }
}
