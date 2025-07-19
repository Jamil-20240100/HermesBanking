namespace HermesBanking.Core.Domain.Entities
{
    public class CreditCard
    {
        public int Id { get; set; }
        public string? CardId { get; set; } 
        public required string ClientId { get; set; }
        public required string ClientFullName { get; set; }
        public required decimal CreditLimit { get; set; }
        public required decimal TotalOwedAmount { get; set; } // Equivalente a Debt
        public string? CVC { get; set; }
        public required bool IsActive { get; set; }
        public DateTime ExpirationDate { get; set; }
        public DateTime CreatedAt { get; set; }

        public string? CreatedByAdminId { get; set; }
        public string? AdminFullName { get; set; }
    }
}
