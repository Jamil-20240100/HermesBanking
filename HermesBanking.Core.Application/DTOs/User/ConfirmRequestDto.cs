namespace HermesBanking.Core.Application.DTOs.User
{
    public class ConfirmRequestDto
    {      
        public required string UserId { get; set; }
        public required string Token { get; set; }

    }
}
