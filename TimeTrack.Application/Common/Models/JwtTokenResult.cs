namespace TimeTrack.Application.Common.Models
{
    public class JwtTokenResult
    {
        public string Token { get; set; } = default!;
        public DateTime ExpiresAtUtc { get; set; }
    }
}
