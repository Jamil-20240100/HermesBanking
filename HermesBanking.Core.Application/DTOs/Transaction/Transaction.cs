namespace HermesBanking.Core.Application.DTOs.Transaction
{
    public class TransactionDTO
    {
        public int Id { get; set; }
        public int SavingsAccountId { get; set; }
        public string Type { get; set; } = null!;
        public decimal Amount { get; set; }
        public string Origin { get; set; } = null!;
        public string Beneficiary { get; set; } = null!;
        public string? CashierId { get; set; }
        public DateTime Date { get; set; }
    }
}
