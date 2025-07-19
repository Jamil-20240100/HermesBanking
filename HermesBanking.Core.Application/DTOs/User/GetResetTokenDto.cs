using System.ComponentModel.DataAnnotations;

namespace HermesBanking.Core.Application.DTOs.User
{
    public class GetResetTokenDto
    {
        [Required]
        public string UserName { get; set; } = null!;
    }
}