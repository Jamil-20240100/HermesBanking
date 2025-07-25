namespace HermesBanking.Core.Application.DTOs.Transaction
{
    public class DisplayTransactionDTO
    {
        public string? Type { get; set; }
        public string? saTransactionType { get; set; } 
        public decimal Amount { get; set; }
        public DateTime Date { get; set; }
        public string? OriginIdentifier { get; set; } 
        public string? DestinationIdentifier { get; set; }
        public string? Description { get; set; }
        public string? TransactionId { get; set; }
        public string? Beneficiary { get; set; }
        public string? Status { get; set; }
    }
}