namespace HermesBanking.Core.Application.DTOs.Cashier
{
    public class ThirdPartyTransferDto
    {
        public string SourceAccountNumber { get; set; } = null!;
        public string DestinationAccountNumber { get; set; } = null!;
        public decimal Amount { get; set; }
        public string CashierId { get; set; } = null!;
    }
}
