using MediatR;
using Microsoft.EntityFrameworkCore;
using TimeTrack.Application.Common.Exceptions;
using TimeTrack.Application.Common.Interfaces;
using TimeTrack.Domain.Entities;

namespace TimeTrack.Application.Features.Users.Commands.ForgotPassword
{
    public class ForgotPasswordCommandHandler
    : IRequestHandler<ForgotPasswordCommand, ForgotPasswordResult>
    {
        private readonly IIdentityService _identityService;
        private readonly IApplicationDbContext _context;

        public ForgotPasswordCommandHandler(
            IIdentityService identityService,
            IApplicationDbContext context)
        {
            _identityService = identityService;
            _context = context;
        }

        public async Task<ForgotPasswordResult> Handle(
            ForgotPasswordCommand request,
            CancellationToken cancellationToken)
        {
            // 1. Find user
            var userId = await _identityService.GetUserIdByEmailAsync(
                request.Email,
                cancellationToken);

            if (userId is null)
            {
                var errors = new Dictionary<string, string[]>
                {
                    ["email"] = new[] { "No user found with this email." }
                };

                throw new ValidationException(errors);
            }

            var nowUtc = DateTime.UtcNow;

            // 2. Optional throttle: avoid spamming reset OTPs
            var lastOtp = await _context.UserOtps
                .Where(o => o.UserId == userId.Value && o.Purpose == "PasswordReset")
                .OrderByDescending(o => o.CreatedAtUtc)
                .FirstOrDefaultAsync(cancellationToken);

            if (lastOtp is not null && lastOtp.CreatedAtUtc > nowUtc.AddSeconds(-60))
            {
                var errors = new Dictionary<string, string[]>
                {
                    ["otp"] = new[] { "Please wait at least 60 seconds before requesting another OTP." }
                };

                throw new ValidationException(errors);
            }

            // 3. Generate OTP (e.g. 6-digit)
            var otpCode = new Random().Next(100000, 999999).ToString();

            var otp = new UserOtp
            {
                UserId = userId.Value,
                Code = otpCode,
                Purpose = "PasswordReset",
                CreatedAtUtc = nowUtc,
                ExpiresAtUtc = nowUtc.AddMinutes(10),
                IsUsed = false,
                AttemptCount = 0
            };

            _context.UserOtps.Add(otp);
            await _context.SaveChangesAsync(cancellationToken);

            return new ForgotPasswordResult
            {
                UserId = userId.Value,
                Email = request.Email,
                OtpCode = otpCode
            };
        }
    }
}
