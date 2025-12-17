using FluentValidation;

namespace TimeTrack.Application.Features.Users.Commands.RegisterUser
{
    public class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
    {
        public RegisterUserCommandValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty()
                .EmailAddress();

            RuleFor(x => x.Password)
                .NotEmpty()
                .MinimumLength(6);

            RuleFor(x => x.FirstName)
                .NotEmpty()
                .MaximumLength(100);

            RuleFor(x => x.LastName)
                .MaximumLength(100);

            RuleFor(x => x.EmployeeCode)
                .NotEmpty()
                .MaximumLength(50);

            RuleFor(x => x.Department)
                .MaximumLength(100)
                .When(x => !string.IsNullOrWhiteSpace(x.Department));

            RuleFor(x => x.Designation)
                .MaximumLength(100)
                .When(x => !string.IsNullOrWhiteSpace(x.Designation));
        }
    }
}
