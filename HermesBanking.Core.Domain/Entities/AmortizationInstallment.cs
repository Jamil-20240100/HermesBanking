namespace HermesBanking.Core.Domain.Entities
{
    public class AmortizationInstallment
    {
        public int Id { get; set; }
        public int LoanId { get; set; }
        public Loan Loan { get; set; }

        public int InstallmentNumber { get; set; }
        public DateTime DueDate { get; set; } // Renamed from PaymentDate for clarity, as this is the scheduled due date
        public decimal InstallmentValue { get; set; }
        public decimal PrincipalAmount { get; set; }
        public decimal InterestAmount { get; set; }
        public decimal RemainingBalance { get; set; } // This should be the loan's balance AFTER this installment is paid

        public decimal AmountPaid { get; set; } // NEW: Tracks how much of this installment has been paid

        public bool IsPaid { get; set; }
        public bool IsOverdue { get; set; }
        public DateTime? PaidDate { get; set; } // Actual date the installment was fully paid
    }
}