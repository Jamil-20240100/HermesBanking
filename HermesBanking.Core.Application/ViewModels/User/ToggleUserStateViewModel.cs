namespace HermesBanking.Core.Application.ViewModels.User
{
    public class ToggleUserStateViewModel
    {
        public required string Id { get; set; }
        public string? Name { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
        public string? UserName { get; set; }
        public string? Password { get; set; }
        public string? Role { get; set; }
        public string? UserId { get; set; }
        public decimal? InitialAmount { get; set; }
        public required bool IsActive { get; set; }
    }
}