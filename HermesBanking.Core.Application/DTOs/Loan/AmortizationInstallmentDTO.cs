namespace HermesBanking.Core.Application.DTOs.Loan
{
    public class AmortizationInstallmentDTO
    {
        public int Id { get; set; }
        public int LoanId { get; set; }
        public int InstallmentNumber { get; set; }
        public DateTime DueDate { get; set; } // Renamed for clarity: scheduled due date
        public decimal InstallmentValue { get; set; } // Total amount due for this installment
        public decimal PrincipalAmount { get; set; }
        public decimal InterestAmount { get; set; }
        public decimal RemainingBalance { get; set; } // Balance of the loan after this installment is paid
        public decimal AmountPaid { get; set; } // NEW: Actual amount paid for this installment
        public bool IsPaid { get; set; } // True if AmountPaid >= InstallmentValue
        public bool IsOverdue { get; set; } // True if DueDate passed and not IsPaid
        public DateTime? PaidDate { get; set; } // Actual date the installment was fully paid
    }
}