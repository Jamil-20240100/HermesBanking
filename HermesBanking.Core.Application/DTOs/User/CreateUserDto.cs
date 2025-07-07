using HermesBanking.Core.Domain.Common.Enums;
using Microsoft.AspNetCore.Http;

namespace HermesBanking.Core.Application.DTOs.User
{
    public class CreateUserDto
    {      
        public required string Name { get; set; }
        public required string LastName { get; set; }
        public required string Email { get; set; }
        public required string UserName { get; set; }
        public required string Password { get; set; }
        public string? Phone { get; set; }
        public required IFormFile ProfileImage { get; set; }
        public required Roles Role { get; set; }
    }
}
