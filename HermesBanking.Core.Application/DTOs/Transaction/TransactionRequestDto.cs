// HermesBanking.Core.Application.DTOs/Transaction/TransactionRequestDto.cs
using HermesBanking.Core.Application.DTOs.SavingsAccount; // Import the specific DTOs
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace HermesBanking.Core.Application.DTOs.Transaction
{
    public class TransactionRequestDto
    {
        [Required(ErrorMessage = "La cuenta de origen es obligatoria.")]
        [DisplayName("Cuenta de Origen")]
        public string SourceAccountNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "La cuenta de destino es obligatoria.")]
        [DisplayName("Cuenta de Destino")]
        public string DestinationAccountNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "El monto es obligatorio.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El monto debe ser mayor que cero.")]
        [DisplayName("Monto")]
        public decimal Amount { get; set; }

        [DisplayName("Descripción")]
        [StringLength(500, ErrorMessage = "La descripción no puede exceder los 500 caracteres.")]
        public string? Description { get; set; }

        // Use your actual SavingsAccountDTO here
        public List<SavingsAccountDTO> AvailableAccounts { get; set; } = new List<SavingsAccountDTO>();
    }
}