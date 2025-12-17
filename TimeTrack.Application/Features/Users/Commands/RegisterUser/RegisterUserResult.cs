namespace TimeTrack.Application.Features.Users.Commands.RegisterUser
{
    public class RegisterUserResult
    {
        public int UserId { get; set; }
        public int EmployeeId { get; set; }
        public string Email { get; set; } = default!;
        public string OtpCode { get; set; } = default!;
    }
}
