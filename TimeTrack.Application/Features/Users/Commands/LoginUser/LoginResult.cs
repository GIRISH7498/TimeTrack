namespace TimeTrack.Application.Features.Users.Commands.LoginUser
{
    public class LoginResult
    {
        public int UserId { get; set; }
        public int? EmployeeId { get; set; }
        public string Email { get; set; } = default!;
        public string Token { get; set; } = default!;
        public DateTime ExpiresAtUtc { get; set; }
    }
}
