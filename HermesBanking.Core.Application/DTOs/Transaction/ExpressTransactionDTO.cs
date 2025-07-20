using System.ComponentModel.DataAnnotations;

namespace HermesBanking.Core.Application.DTOs.Transaction
{
    public class ExpressTransactionDTO
    {
        public string SenderAccountNumber { get; set; }
        public string ReceiverAccountNumber { get; set; }
        public decimal Amount { get; set; }
        
        public string Type { get; set; }
        public int SavingsAccountId { get; set; }
    }
}
