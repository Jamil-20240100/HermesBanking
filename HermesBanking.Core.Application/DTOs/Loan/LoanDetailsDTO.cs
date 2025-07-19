namespace HermesBanking.Core.Application.DTOs.Loan
{
    public class LoanDetailDTO
    {
        public string PrestamoId { get; set; } = null!;
        public List<AmortizationInstallmentDTO> TablaAmortizacion { get; set; } = new();
    }

}
