using MediatR;
using Microsoft.EntityFrameworkCore;
using TimeTrack.Application.Common.Exceptions;
using TimeTrack.Application.Common.Interfaces;
using TimeTrack.Application.Features.Users.Commands.LoginUser;
using TimeTrack.Domain.Entities;

namespace TimeTrack.Application.Features.Users.Commands.ResendAuthOtp
{
    public class ResendAuthOtpCommandHandler
    : IRequestHandler<ResendAuthOtpCommand, LoginOtpResult>
    {
        private readonly IIdentityService _identityService;
        private readonly IApplicationDbContext _context;

        public ResendAuthOtpCommandHandler(
            IIdentityService identityService,
            IApplicationDbContext context)
        {
            _identityService = identityService;
            _context = context;
        }

        public async Task<LoginOtpResult> Handle(
            ResendAuthOtpCommand request,
            CancellationToken cancellationToken)
        {
            var userId = await _identityService.GetUserIdByEmailAsync(
                request.Email,
                cancellationToken);

            if (userId is null)
            {
                ThrowUserNotFound();
            }

            var nowUtc = DateTime.UtcNow;

            var lastOtp = await _context.UserOtps
                .Where(o => o.UserId == userId.Value && o.Purpose == "Auth")
                .OrderByDescending(o => o.CreatedAtUtc)
                .FirstOrDefaultAsync(cancellationToken);

            if (lastOtp is not null && lastOtp.CreatedAtUtc > nowUtc.AddSeconds(-60))
            {
                var errors = new Dictionary<string, string[]>
                {
                    ["otp"] = new[] { "Please wait at least 60 seconds before requesting a new OTP." }
                };

                throw new ValidationException(errors);
            }

            var employee = await _context.Employees
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.UserId == userId.Value, cancellationToken);

            var otpCode = new Random().Next(100000, 999999).ToString();

            var newOtp = new UserOtp
            {
                UserId = userId.Value,
                Code = otpCode,
                Purpose = "Auth",
                CreatedAtUtc = nowUtc,
                ExpiresAtUtc = nowUtc.AddMinutes(5),
                IsUsed = false,
                AttemptCount = 0
            };

            _context.UserOtps.Add(newOtp);
            await _context.SaveChangesAsync(cancellationToken);

            return new LoginOtpResult
            {
                UserId = userId.Value,
                EmployeeId = employee?.Id,
                Email = request.Email,
                OtpCode = otpCode
            };
        }

        private static void ThrowUserNotFound()
        {
            var errors = new Dictionary<string, string[]>
            {
                ["email"] = new[] { "No user found with this email." }
            };

            throw new ValidationException(errors);
        }
    }
}
