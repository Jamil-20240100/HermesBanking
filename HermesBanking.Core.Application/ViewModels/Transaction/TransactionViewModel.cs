public class TransactionViewModel
{
    public string SourceAccountId { get; set; }
    public string DestinationAccountId { get; set; }
    public decimal Amount { get; set; }

    // Campos de tipo para otros tipos de transacciones (tarjetas, préstamos, etc.)
    public List<SavingsAccountDTO> SavingsAccounts { get; set; }
    public List<BeneficiaryDTO> Beneficiaries { get; set; }
    public List<CreditCardDTO> CreditCards { get; set; }
    public List<LoanDTO> Loans { get; set; }
}
