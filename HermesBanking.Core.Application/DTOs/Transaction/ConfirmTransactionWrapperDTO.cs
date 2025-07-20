using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HermesBanking.Core.Application.DTOs.Transaction
{
    public class ConfirmTransactionWrapperDTO
    {
        public string Type { get; set; } // "Express", "CreditCard", "Loan", "Beneficiary"
        public string JsonPayload { get; set; } // JSON serializado del DTO correspondiente
    }

}
