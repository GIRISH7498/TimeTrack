namespace TimeTrack.Application.Features.Users.Commands.LoginUser
{
    public class LoginOtpResult
    {
        public int UserId { get; set; }
        public int? EmployeeId { get; set; }
        public string Email { get; set; } = default!;
        public string OtpCode { get; set; } = default!;
    }
}
