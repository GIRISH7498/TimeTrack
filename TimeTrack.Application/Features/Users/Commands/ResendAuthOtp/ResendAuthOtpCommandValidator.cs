using FluentValidation;

namespace TimeTrack.Application.Features.Users.Commands.ResendAuthOtp
{
    public class ResendAuthOtpCommandValidator : AbstractValidator<ResendAuthOtpCommand>
    {
        public ResendAuthOtpCommandValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty()
                .EmailAddress();
        }
    }
}
