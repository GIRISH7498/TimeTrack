using MediatR;
using Microsoft.EntityFrameworkCore;
using TimeTrack.Application.Common.Exceptions;
using TimeTrack.Application.Common.Interfaces;

namespace TimeTrack.Application.Features.Users.Commands.ResetPassword
{
    public class ResetPasswordCommandHandler
    : IRequestHandler<ResetPasswordCommand, ResetPasswordResult>
    {
        private readonly IIdentityService _identityService;
        private readonly IApplicationDbContext _context;

        public ResetPasswordCommandHandler(
            IIdentityService identityService,
            IApplicationDbContext context)
        {
            _identityService = identityService;
            _context = context;
        }

        public async Task<ResetPasswordResult> Handle(
            ResetPasswordCommand request,
            CancellationToken cancellationToken)
        {
            var userId = await _identityService.GetUserIdByEmailAsync(
                request.Email,
                cancellationToken);

            if (userId is null)
            {
                ThrowInvalidOtp();
            }

            var nowUtc = DateTime.UtcNow;

            // 2. Get latest unused PasswordReset OTP
            var otp = await _context.UserOtps
                .Where(o => o.UserId == userId.Value
                            && o.Purpose == "PasswordReset"
                            && !o.IsUsed)
                .OrderByDescending(o => o.CreatedAtUtc)
                .FirstOrDefaultAsync(cancellationToken);

            if (otp is null)
            {
                ThrowInvalidOtp();
            }

            if (otp.ExpiresAtUtc < nowUtc)
            {
                ThrowInvalidOtp("OTP has expired. Please request a new one.");
            }

            if (!string.Equals(otp.Code, request.OtpCode, StringComparison.Ordinal))
            {
                otp.AttemptCount += 1;
                await _context.SaveChangesAsync(cancellationToken);

                ThrowInvalidOtp();
            }

            otp.IsUsed = true;
            otp.UsedAtUtc = nowUtc;

            await _identityService.ResetPasswordAsync(userId.Value, request.NewPassword, cancellationToken);

            await _context.SaveChangesAsync(cancellationToken);

            return new ResetPasswordResult
            {
                Email = request.Email
            };
        }

        private static void ThrowInvalidOtp(string? message = null)
        {
            var errors = new Dictionary<string, string[]>
            {
                ["otp"] = new[] { message ?? "Invalid email or OTP." }
            };

            throw new ValidationException(errors);
        }
    }
}
