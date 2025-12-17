namespace TimeTrack.Application.Features.Users.Commands.ForgotPassword
{
    public class ForgotPasswordResult
    {
        public int UserId { get; set; }
        public string Email { get; set; } = default!;
        public string OtpCode { get; set; } = default!;
    }
}
