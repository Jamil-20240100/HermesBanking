using HermesBanking.Core.Application.DTOs.Loan;
using HermesBanking.Core.Application.DTOs.SavingsAccount;
using HermesBanking.Core.Application.ViewModels.Loan;
using HermesBanking.Core.Application.ViewModels.SavingsAccount;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HermesBanking.Core.Application.ViewModels.Transaction
{
    public class PayLoanViewModel
    {
        [Required]
        public string LoanId { get; set; }

        [Required]
        public string SavingsAccountNumber { get; set; }

        [Required]
        [Range(1, double.MaxValue)]
        public decimal Amount { get; set; }



        public List<SavingsAccountViewModel>? AvailableAccounts { get; set; }
        public List<LoanViewModel> AvailableLoans { get; set; }

        public bool HasError { get; set; } = false;
        public string? ErrorMessage { get; set; }
       
    }

}
