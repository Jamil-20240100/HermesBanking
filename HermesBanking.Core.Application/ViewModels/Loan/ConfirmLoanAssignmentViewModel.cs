using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HermesBanking.Core.Application.ViewModels.Loan
{
    public class ConfirmLoanAssignmentViewModel
    {
        public string ClientId { get; set; }
        public decimal Amount { get; set; }
        public decimal InterestRate { get; set; }
        public int LoanTermMonths { get; set; }
        public string CreatedByAdminId { get; set; }
        public string AdminFullName { get; set; }
        public string RiskMessage { get; set; }
        public bool IsExistingHighRisk { get; set; }
    }
}
