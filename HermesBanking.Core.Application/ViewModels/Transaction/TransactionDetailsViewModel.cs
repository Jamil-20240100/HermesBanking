namespace HermesBanking.Core.Application.ViewModels.Transaction
{
    public class TransactionDetailsViewModel
    {
        // Comunes
        public string OriginAccount { get; set; }
        public string DestinationAccount { get; set; }
        public decimal Amount { get; set; }
        public string Type { get; set; }
        public string TransactionType { get; set; }
        public string Description { get; set; }
        public DateTime Date { get; set; }

        // Específicas para cada tipo
        public string CreditCardId { get; set; }
        public string LoanId { get; set; }
        public string BeneficiaryName { get; set; }
    }
}
