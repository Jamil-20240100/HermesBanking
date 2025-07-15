namespace HermesBanking.Core.Application.DTOs.Cashier
{
    public class WithdrawDto
    {
        public string AccountNumber { get; set; } = null!;
        public decimal Amount { get; set; }
        public string CashierId { get; set; } = null!;
    }
}
