namespace HermesBanking.Core.Application.ViewModels.Loan
{
    public class ClientSelectionViewModel
    {
        public string? Id { get; set; }
        public string? IdentificationNumber { get; set; }
        public string? Name { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
        public decimal CurrentDebtAmount { get; set; }
        public bool IsSelected { get; set; }
    }
}