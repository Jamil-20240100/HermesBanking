using HermesBanking.Core.Application.DTOs.CreditCard;
using HermesBanking.Core.Application.DTOs.SavingsAccount;
using HermesBanking.Core.Application.ViewModels.CreditCard;
using HermesBanking.Core.Application.ViewModels.SavingsAccount;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.ComponentModel.DataAnnotations;


namespace HermesBanking.Core.Application.ViewModels.Transaction
{
    

    public class PayCreditCardViewModel
    {
        [Required]
        public string CreditCardId { get; set; }

        [Required]
        public string FromAccountId { get; set; }

        [Required]
        [Range(1, double.MaxValue)]
        public decimal Amount { get; set; }



        public List<SavingsAccountViewModel>? AvailableAccounts { get; set; }
        public List<CreditCardViewModel> AvailableCreditCards { get; set; }

        public bool HasError { get; set; } = false;
        public string? ErrorMessage { get; set; }
    }
}
