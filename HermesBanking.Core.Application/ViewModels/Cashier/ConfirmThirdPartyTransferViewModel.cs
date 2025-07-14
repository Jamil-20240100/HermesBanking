namespace HermesBanking.Core.Application.ViewModels.Cashier
{
    public class ConfirmThirdPartyTransferViewModel
    {
        public string SourceAccountNumber { get; set; } = null!;
        public string DestinationAccountNumber { get; set; } = null!;
        public string DestinationClientFullName { get; set; } = null!;
        public decimal Amount { get; set; }
    }
}
