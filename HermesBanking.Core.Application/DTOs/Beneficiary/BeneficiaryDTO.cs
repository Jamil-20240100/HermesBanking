namespace HermesBanking.Core.Application.DTOs.Beneficiary
{
    public class BeneficiaryDTO
    {
        public int Id { get; set; }
        public required string ClientId { get; set; }
        public string BeneficiaryAccountNumber { get; set; } = string.Empty;
        public required string Name { get; set; }
        public required string LastName { get; set; }
        public DateTime CreatedAt { get; set; }
        public string DisplayText => $"{Name} {LastName} ({BeneficiaryAccountNumber})";

    }
}