using System.ComponentModel.DataAnnotations;

namespace HermesBanking.Core.Application.DTOs.CreditCard
{
    public class CreditCardDTO
    {
        public int Id { get; set; }
        public string? CardId { get; set; } // Note: You might want to use CardNumber for display/selection
        public required string ClientId { get; set; }
        public string? ClientFullName { get; set; }
        public string? ClientIdentification { get; set; }
        public required decimal CreditLimit { get; set; }
        public required decimal TotalOwedAmount { get; set; }
        public string? CVC { get; set; }
        public required bool IsActive { get; set; }
        public DateTime ExpirationDate { get; set; }
        public DateTime CreatedAt { get; set; }

        public string? CreatedByAdminId { get; set; }
        public string? AdminFullName { get; set; }

        // Added for UI display in dropdowns
        public string DisplayText => $"{CardId ?? "N/A"} (Deuda: {TotalOwedAmount:C} | Límite: {CreditLimit:C})";

    }
}