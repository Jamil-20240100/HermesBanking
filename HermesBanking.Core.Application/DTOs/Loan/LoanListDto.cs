namespace HermesBanking.Core.Application.DTOs.Loan
{
    public class LoanListDto
    {
        public int PaginaActual { get; set; }
        public int TotalPaginas { get; set; }
        public List<LoanDTO> Prestamos { get; set; } = new();
    }

}
