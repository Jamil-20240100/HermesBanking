using System.ComponentModel.DataAnnotations;


namespace HermesBanking.Core.Application.DTOs.Transaction
{
    public class PayLoanDTO
    {
        public int Id { get; set; }
        public string FromAccountNumber { get; set; }

        [Required]
        public string LoanCode { get; set; }
        public decimal Amount { get; set; }
        public int? SavingsAccountId { get; set; }

    }
}
