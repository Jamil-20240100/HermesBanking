﻿namespace HermesBanking.Core.Application.ViewModels.User
{
    public class UserViewModel
    {
        public required string Id { get; set; }
        public required string Name { get; set; }
        public required string LastName { get; set; }
        public required string Email { get; set; }
        public required string UserName { get; set; }
        public required string Role { get; set; }
        public decimal? InitialAmount { get; set; }
        public required string UserId { get; set; }
        public required bool IsActive { get; set; }
        public string FullName => $"{Name} {LastName}";
        public decimal TotalDebt { get; set; }
    }
}
