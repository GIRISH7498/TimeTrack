using MediatR;
using TimeTrack.Application.Features.Users.Commands.LoginUser;

namespace TimeTrack.Application.Features.Users.Commands.VerifyRegistrationOtp
{
    public record VerifyRegistrationOtpCommand(
        string Email,
        string OtpCode
        ) : IRequest<LoginResult>;
}
