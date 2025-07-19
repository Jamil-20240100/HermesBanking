using System.ComponentModel.DataAnnotations;

namespace HermesBanking.Core.Application.ViewModels.Beneficiary
{
    public class SaveBeneficiaryViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El número de cuenta es obligatorio.")]
        public string  BeneficiaryAccountNumber { get; set; }
        public string? ClientId { get; set; }
        public bool HasError { get; set; }
        public string? ErrorMessage { get; set; }
    }
}