namespace HermesBanking.Core.Application.ViewModels.Cashier
{
    public class ConfirmDepositViewModel
    {
        public string AccountNumber { get; set; } = null!;

        public string ClientFullName { get; set; } = null!;

        public decimal Amount { get; set; }
    }
}
