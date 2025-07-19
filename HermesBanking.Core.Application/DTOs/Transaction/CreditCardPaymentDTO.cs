namespace HermesBanking.Core.Application.DTOs
{
    public class CreditCardPaymentDTO
    {
        public int SavingsAccountId { get; set; }
        public int CreditCardId { get; set; }
        public decimal Amount { get; set; }
        public string ClientId { get; set; }
    }
}
