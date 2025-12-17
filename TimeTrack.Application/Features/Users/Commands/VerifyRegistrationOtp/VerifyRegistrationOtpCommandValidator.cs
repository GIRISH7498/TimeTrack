using FluentValidation;

namespace TimeTrack.Application.Features.Users.Commands.VerifyRegistrationOtp
{
    public class VerifyRegistrationOtpCommandValidator : AbstractValidator<VerifyRegistrationOtpCommand>
    {
        public VerifyRegistrationOtpCommandValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty()
                .EmailAddress();

            RuleFor(x => x.OtpCode)
                .NotEmpty()
                .Length(4, 10);
        }
    }
}
