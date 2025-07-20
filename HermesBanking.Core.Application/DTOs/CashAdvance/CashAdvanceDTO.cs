namespace HermesBanking.Core.Application.DTOs.CashAdvance
{
    public class CashAdvanceDTO
    {
        public int SourceCreditCardId { get; set; }
        public int DestinationSavingsAccountId { get; set; }
        public decimal Amount { get; set; }
        public string? ClientId { get; set; }
    }
}