namespace HermesBanking.Core.Application.DTOs.HermesPay
{
    public class HermesPayRequest
    {
        public string? CardNumber { get; set; }
        public string? MonthExpirationCard { get; set; }
        public string? YearExpirationCard { get; set; }
        public string? CVC { get; set; }
        public decimal? Ammount { get; set; }
        public int? CommerceId { get; set; }
    }
}
