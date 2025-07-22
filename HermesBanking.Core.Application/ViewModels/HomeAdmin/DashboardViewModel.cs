namespace HermesBanking.Core.Application.ViewModels.HomeAdmin
{
    public class DashboardViewModel
    {
        // Propiedades existentes para Tarjetas de Crédito
        public int TotalIssuedCreditCards { get; set; }
        public int ActiveCreditCards { get; set; }
        public int InactiveCreditCards { get; set; }

        // Propiedades existentes para Cuentas de Ahorro
        public int TotalSavingsAccounts { get; set; }

        // Propiedades existentes para Préstamos
        public int ActiveLoans { get; set; }
        public decimal AverageClientDebt { get; set; }

        // Propiedades existentes para Clientes
        public int ActiveClients { get; set; }
        public int InactiveClients { get; set; }

        public int TotalTransactions { get; set; }
        public int TransactionsToday { get; set; }
        public decimal TotalPaymentsAmount { get; set; } // Para el "Total Acumulado" de pagos
        public decimal PaymentsProcessedToday { get; set; } // Para "Procesados Hoy" de pagos
    }
}