using System.ComponentModel.DataAnnotations;

namespace HermesBanking.Core.Application.DTOs.Transaction
{
    public class TransactionDTO
    {
        public int Id { get; set; }

        public int SavingsAccountId { get; set; }

        [Required]
        public string Type { get; set; } = null!;

        public decimal Amount { get; set; }

        [Required]
        public string Origin { get; set; } = null!;

        [Required]
        public string Beneficiary { get; set; } = null!;

        public string? CashierId { get; set; }

        public DateTime Date { get; set; }

        public string Status { get; set; } = null!;

        public string Description { get; set; } = null!;
    }
}
