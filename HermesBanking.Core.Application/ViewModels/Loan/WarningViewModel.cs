namespace HermesBanking.Core.Application.ViewModels.Loan
{
    public class WarningViewModel
    {
        public string ClientId { get; set; }
        public string Message { get; set; }
        public string ContinueActionUrl { get; set; }
        public string CancelActionUrl { get; set; }
    }
}