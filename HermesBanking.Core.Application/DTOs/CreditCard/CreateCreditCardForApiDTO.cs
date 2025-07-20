namespace HermesBanking.Core.Application.DTOs.CreditCard
{
    public class CreateCreditCardForApiDTO
    {
        public string ClienteId { get; set; } = null!;
        public decimal Limite { get; set; }
    }

}
