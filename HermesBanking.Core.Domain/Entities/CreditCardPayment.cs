namespace HermesBanking.Core.Domain.Entities
{
    public class CreditCardPayment
    {
        public int Id { get; set; }
        public int SavingsAccountId { get; set; }
        public int CreditCardId { get; set; }
        public decimal Amount { get; set; }
        public DateTime PaymentDate { get; set; }

        public virtual SavingsAccount SavingsAccount { get; set; }
        public virtual CreditCard CreditCard { get; set; }
    }
}
