using HermesBanking.Core.Application.ViewModels.SavingsAccount;

namespace HermesBanking.Core.Application.ViewModels.Beneficiary
{
    public class BeneficiaryViewModel
    {
        public int Id { get; set; }
        public string? BeneficiaryAccountNumber { get; set; }
        public string? Name { get; set; }
        public string? LastName { get; set; }

        public List<BeneficiaryViewModel> AvailableBeneficiaries { get; set; }
        public List<SavingsAccountViewModel> AvailableAccounts { get; set; }
    }
}