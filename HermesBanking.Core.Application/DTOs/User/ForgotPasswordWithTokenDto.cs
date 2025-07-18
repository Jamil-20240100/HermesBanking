using System.ComponentModel.DataAnnotations;

namespace HermesBanking.Core.Application.DTOs.User
{
    public class ForgotPasswordWithTokenDto
    {
        [Required]
        public string UserName { get; set; } = null!;
    }

}
