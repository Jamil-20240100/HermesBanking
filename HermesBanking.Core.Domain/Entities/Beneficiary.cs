namespace HermesBanking.Core.Domain.Entities
{
    public class Beneficiary
    {
        public int Id { get; set; }
        public required string ClientId { get; set; }
        public string? BeneficiaryAccountNumber { get; set; }   
        public required string Name { get; set; }
        public required string LastName { get; set; }
        public DateTime CreatedAt { get; set; }


        
    }
}
