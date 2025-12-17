using MediatR;
using TimeTrack.Application.Features.Users.Commands.LoginUser;

namespace TimeTrack.Application.Features.Users.Commands.ResendAuthOtp
{
    public record ResendAuthOtpCommand(string Email) : IRequest<LoginOtpResult>;
}
