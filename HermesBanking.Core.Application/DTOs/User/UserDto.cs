namespace HermesBanking.Core.Application.DTOs.User
{
    public class UserDto
    {
        public required string Id { get; set; }
        public required string Name { get; set; }
        public required string LastName { get; set; }
        public required string Email { get; set; }
        public required string UserName { get; set; }
        public bool? isVerified { get; set; }
        public required string Role { get; set; }
        public required string UserId { get; set; }
        public double? InitialAmount { get; set; }
        public required bool IsActive { get; set; }
    }
}
