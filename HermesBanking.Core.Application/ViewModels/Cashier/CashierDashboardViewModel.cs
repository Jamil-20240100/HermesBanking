using HermesBanking.Core.Application.ViewModels.SavingsAccount;

namespace HermesBanking.Core.Application.ViewModels.Cashier
{
    public class CashierDashboardViewModel
    {
        public int TotalTransactions { get; set; }
        public int TotalDeposits { get; set; }
        public int TotalWithdrawals { get; set; }
        public int TotalPayments { get; set; }

        public List<SavingsAccountViewModel> Accounts { get; set; }
        public DateTime TransactionDate { get; set; }
    }
}
