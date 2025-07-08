using Microsoft.AspNetCore.Identity;

namespace HermesBanking.Infrastructure.Identity.Entities
{
    public class AppUser : IdentityUser
    {
        public required string Name { get; set; }
        public required string LastName { get; set; }
        public required string UserId { get; set; } //cedula
        public double? InitialAmount { get; set; }
        public required bool IsActive { get; set; }
    }
}
