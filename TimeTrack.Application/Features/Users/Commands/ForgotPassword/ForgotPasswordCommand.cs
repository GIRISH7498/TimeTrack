using MediatR;

namespace TimeTrack.Application.Features.Users.Commands.ForgotPassword
{
    public record ForgotPasswordCommand(string Email) : IRequest<ForgotPasswordResult>;
}
