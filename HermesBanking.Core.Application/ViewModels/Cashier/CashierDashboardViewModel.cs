using HermesBanking.Core.Domain.Entities;
using System.Security.Principal;

namespace HermesBanking.Core.Application.ViewModels.Cashier
{
    public class CashierDashboardViewModel
    {
        public int TotalTransactions { get; set; }
        public int TotalDeposits { get; set; }
        public int TotalWithdrawals { get; set; }
        public int TotalPayments { get; set; } 

        public List<HermesBanking.Core.Domain.Entities.SavingsAccount> Accounts { get; set; } = new();
    }
}
