using HermesBanking.Core.Application.DTOs.SavingsAccount;
using HermesBanking.Core.Application.ViewModels.SavingsAccount;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.ComponentModel.DataAnnotations;

namespace HermesBanking.Core.Application.ViewModels
{
    public class ExpressTransactionViewModel
    {
        [Required(ErrorMessage = "La cuenta de origen es obligatoria.")]
        [Display(Name = "Tu Cuenta (Origen)")]
        public string SenderAccountNumber { get; set; } = null!;

        [Required(ErrorMessage = "La cuenta destino es obligatoria.")]
        [Display(Name = "Cuenta Destino")]
        public string ReceiverAccountNumber { get; set; } = null!;

        [Required(ErrorMessage = "El monto es obligatorio.")]
        [Range(1, double.MaxValue, ErrorMessage = "El monto debe ser mayor que cero.")]
        [Display(Name = "Monto a Transferir (RD$)")]
        public decimal Amount { get; set; }



        public List<SavingsAccountViewModel>? AvailableAccounts { get; set; }

        public bool HasError { get; set; } = false;
        public string? ErrorMessage { get; set; }
    }
}
