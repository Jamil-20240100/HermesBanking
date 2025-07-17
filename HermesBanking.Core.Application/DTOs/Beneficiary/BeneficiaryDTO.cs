namespace HermesBanking.Core.Application.DTOs.Beneficiary
{
    public class BeneficiaryDTO
    {
        public int Id { get; set; }
        public required string ClientId { get; set; }
        public required string BeneficiaryAccountNumber { get; set; }
        public required string Name { get; set; }
        public required string LastName { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}