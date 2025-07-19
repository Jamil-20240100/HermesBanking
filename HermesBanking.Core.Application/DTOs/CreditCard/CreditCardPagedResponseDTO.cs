namespace HermesBanking.Core.Application.DTOs.CreditCard
{
    public class CreditCardPagedResponseDTO
    {
        public List<CreditCardDTO> Data { get; set; }
        public PaginationDTO Paginacion { get; set; }
    }
}
