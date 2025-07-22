using HermesBanking.Core.Application.DTOs.Loan;
using HermesBanking.Core.Application.DTOs.SavingsAccount;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace HermesBanking.Core.Application.DTOs.Transaction
{

    public class LoanPaymentDto
    {
        [Required(ErrorMessage = "La cuenta de origen es obligatoria.")]
        [DisplayName("Cuenta de Origen")]
        public string SourceAccountNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "El identificador del préstamo es obligatorio.")]
        [DisplayName("Identificador del Préstamo")]
        public string LoanIdentifier { get; set; } = string.Empty;

        [Required(ErrorMessage = "El monto es obligatorio.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El monto debe ser mayor que cero.")]
        [DisplayName("Monto a Pagar")]
        public decimal Amount { get; set; }

        [DisplayName("Descripción")]
        [StringLength(500, ErrorMessage = "La descripción no puede exceder los 500 caracteres.")]
        public string? Description { get; set; }

        public List<SavingsAccountDTO> AvailableAccounts { get; set; } = new List<SavingsAccountDTO>();
        public List<LoanDTO> AvailableLoans { get; set; } = new List<LoanDTO>();
    }
}