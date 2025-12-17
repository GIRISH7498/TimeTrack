using MediatR;

namespace TimeTrack.Application.Features.Users.Commands.RegisterUser
{
    public record RegisterUserCommand(
        string Email,
        string Password,
        string FirstName,
        string LastName,
        string EmployeeCode,
        string? Department,
        string? Designation,
        DateTime? JoiningDate
    ) : IRequest<RegisterUserResult>;
}
