using MediatR;

namespace TimeTrack.Application.Features.Users.Commands.LoginUser;

public record LoginCommand(
    string Email,
    string Password
) : IRequest<LoginOtpResult>;
