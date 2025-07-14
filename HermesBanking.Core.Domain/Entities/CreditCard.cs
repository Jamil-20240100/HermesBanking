namespace HermesBanking.Core.Domain.Entities
{
    public class CreditCard
    {
        public int Id { get; set; }
        public string CardNumber { get; set; } = null!; // 16 dígitos
        public decimal Balance { get; set; } // deuda actual
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }

        // Foreign Key
        public string ClientId { get; set; } = null!;
    }
}
