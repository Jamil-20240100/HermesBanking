public class PayBeneficiaryDTO
{
    public string FromAccountNumber { get; set; }
    public string BeneficiaryAccountNumber { get; set; }
    public decimal Amount { get; set; }
    public int? SavingsAccountId { get; set; }

}