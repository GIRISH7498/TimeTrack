using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using TimeTrack.Application.Common.Exceptions;
using TimeTrack.Application.Common.Interfaces;
using TimeTrack.Application.Features.Users.Commands.LoginUser;

namespace TimeTrack.Application.Features.Users.Commands.VerifyRegistrationOtp
{
    public class VerifyRegistrationOtpCommandHandler
    : IRequestHandler<VerifyRegistrationOtpCommand, LoginResult>
    {
        private readonly IIdentityService _identityService;
        private readonly IJwtTokenService _jwtTokenService;
        private readonly IApplicationDbContext _context;

        public VerifyRegistrationOtpCommandHandler(
            IIdentityService identityService,
            IJwtTokenService jwtTokenService,
            IApplicationDbContext context)
        {
            _identityService = identityService;
            _jwtTokenService = jwtTokenService;
            _context = context;
        }

        public async Task<LoginResult> Handle(
            VerifyRegistrationOtpCommand request,
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

            var otp = await _context.UserOtps
                .Where(o => o.UserId == userId!.Value
                            && o.Purpose == "Auth"
                            && !o.IsUsed)
                .OrderByDescending(o => o.CreatedAtUtc)
                .FirstOrDefaultAsync(cancellationToken);

            if (otp is null)
            {
                ThrowInvalidOtp();
            }

            if (otp?.ExpiresAtUtc < nowUtc)
            {
                ThrowInvalidOtp("OTP has expired. Please request a new one.");
            }

            if (!string.Equals(otp?.Code, request.OtpCode, StringComparison.Ordinal))
            {
                otp?.AttemptCount += 1;
                await _context.SaveChangesAsync(cancellationToken);

                ThrowInvalidOtp();
            }

            otp?.IsUsed = true;
            otp?.UsedAtUtc = nowUtc;

            await _identityService.ActivateUserAsync(userId!.Value, cancellationToken);

            await _context.SaveChangesAsync(cancellationToken);

            var employee = await _context.Employees
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.UserId == userId.Value, cancellationToken);

            var tokenResult = await _jwtTokenService.GenerateTokenAsync(
                userId.Value,
                request.Email,
                cancellationToken);

            return new LoginResult
            {
                UserId = userId.Value,
                EmployeeId = employee?.Id,
                Email = request.Email,
                Token = tokenResult.Token,
                ExpiresAtUtc = tokenResult.ExpiresAtUtc
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
