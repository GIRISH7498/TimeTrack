using MediatR;

namespace TimeTrack.Application.Features.Users.Commands.ResetPassword
{
    public record ResetPasswordCommand(
        string Email,
        string OtpCode,
        string NewPassword,
        string ConfirmPassword
    ) : IRequest<ResetPasswordResult>;
}
