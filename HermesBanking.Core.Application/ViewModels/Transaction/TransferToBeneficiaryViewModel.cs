using HermesBanking.Core.Application.DTOs.Beneficiary;
using HermesBanking.Core.Application.DTOs.SavingsAccount;
using HermesBanking.Core.Application.ViewModels.Beneficiary;
using HermesBanking.Core.Application.ViewModels.SavingsAccount;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.ComponentModel.DataAnnotations;

namespace HermesBanking.Core.Application.ViewModels.Transaction
{
    

    public class TransferToBeneficiaryViewModel
    {
        [Required(ErrorMessage = "Debe seleccionar un beneficiario.")]
        public string BeneficiaryId { get; set; }  // Id del beneficiario seleccionado

        [Required(ErrorMessage = "Debe seleccionar una cuenta origen.")]
        public string FromAccountId { get; set; }  // Cuenta de ahorro origen

        [Required(ErrorMessage = "Debe ingresar un monto válido.")]
        [Range(1, double.MaxValue, ErrorMessage = "El monto debe ser mayor que cero.")]
        public decimal Amount { get; set; }



        public List<SavingsAccountViewModel>? AvailableAccounts { get; set; }
        public List<BeneficiaryViewModel> AvailableBeneficiaries { get; set; }

        public bool HasError { get; set; } = false;
        public string? ErrorMessage { get; set; }
        
    }

}
