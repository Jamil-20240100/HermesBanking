namespace HermesBanking.Core.Application.ViewModels.HomeAdmin
{
    public class DashboardViewModel
    {
        public int TotalIssuedCreditCards { get; set; }
        public int ActiveCreditCards { get; set; }
        public int InactiveCreditCards { get; set; }
        public int TotalSavingsAccounts { get; set; }
        public int ActiveLoans { get; set; }
        public decimal AverageClientDebt { get; set; }
        public int ActiveClients { get; set; } // New property for active clients
        public int InactiveClients { get; set; } // New property for inactive clients
        // Add properties for other metrics as you get the services for them
    }
}