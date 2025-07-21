
namespace HermesBanking.Core.Application.DTOs.Email
{
    public class EmailRequestDto
    {
        public string? To { get; set; }
        public required string Subject { get; set; }
        public required string HtmlBody { get; set; }
        public List<string>? ToRange { get; set; } = [];

        public bool IsValid()
        {
            throw new NotImplementedException();
        }
    }
}
