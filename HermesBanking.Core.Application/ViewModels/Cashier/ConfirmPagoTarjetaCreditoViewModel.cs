namespace HermesBanking.Core.Application.ViewModels.Cashier
{
    public class ConfirmPagoTarjetaCreditoViewModel
    {
        public string AccountNumber { get; set; } = null!;
        public string CardNumber { get; set; } = null!;
        public string ClientFullName { get; set; } = null!;
        public decimal Amount { get; set; }
        public decimal DeudaActual { get; set; }
    }
}
