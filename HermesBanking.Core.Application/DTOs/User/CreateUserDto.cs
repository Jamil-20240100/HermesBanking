using HermesBanking.Core.Domain.Common.Enums;

namespace HermesBanking.Core.Application.DTOs.User
{
    public class CreateUserDto
    {
        public required string Name { get; set; }
        public required string LastName { get; set; }
        public required string Email { get; set; }
        public required string UserName { get; set; }
        public required string Password { get; set; }
        public required Roles Role { get; set; }
        public required string UserId { get; set; }
        public double? InitialAmount { get; set; }
        public required bool IsActive { get; set; }

    }
}
