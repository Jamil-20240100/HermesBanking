namespace HermesBanking.Core.Application.DTOs.Commerce
{
    public class CommercePagedResponseDTO
    {
        public List<CommerceDTO> Data { get; set; }
        public PaginationDTO Paginacion { get; set; }
    }

    public class PaginationDTO
    {
        public int PaginaActual { get; set; }
        public int TotalPaginas { get; set; }
        public int TotalRegistros { get; set; }
    }
}
