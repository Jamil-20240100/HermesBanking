namespace HermesBanking.Core.Domain.Entities
{
    public class Loan
    {
        public int Id { get; set; }

        public string LoanNumber { get; set; } = null!; // 9 dígitos

        public decimal TotalAmount { get; set; }

        public decimal RemainingAmount { get; set; }

        public bool IsCompleted { get; set; }

        public DateTime CreatedAt { get; set; }

        public string ClientId { get; set; } = null!;

        public List<LoanInstallment> Installments { get; set; } = [];
    }
}
