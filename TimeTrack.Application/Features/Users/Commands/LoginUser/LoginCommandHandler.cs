using MediatR;
using Microsoft.EntityFrameworkCore;
using TimeTrack.Application.Common.Exceptions;
using TimeTrack.Application.Common.Interfaces;
using TimeTrack.Domain.Entities;

namespace TimeTrack.Application.Features.Users.Commands.LoginUser;

public class LoginCommandHandler : IRequestHandler<LoginCommand, LoginOtpResult>
{
    private readonly IIdentityService _identityService;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IApplicationDbContext _context;

    public LoginCommandHandler(
        IIdentityService identityService,
        IJwtTokenService jwtTokenService,
        IApplicationDbContext context)
    {
        _identityService = identityService;
        _jwtTokenService = jwtTokenService;
        _context = context;
    }

    public async Task<LoginOtpResult> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var userId = await _identityService.GetUserIdByEmailAsync(request.Email, cancellationToken);

        if (userId is null)
        {
            ThrowInvalidCredentials();
        }

        var passwordValid = await _identityService.CheckPasswordAsync(
            request.Email,
            request.Password,
            cancellationToken);

        if (!passwordValid)
        {
            ThrowInvalidCredentials();
        }

        var employee = await _context.Employees
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.UserId == userId.Value, cancellationToken);

        var nowUtc = DateTime.UtcNow;
        var otpCode = new Random().Next(100000, 999999).ToString(); // 6-digit

        var otp = new UserOtp
        {
            UserId = userId.Value,
            Code = otpCode,
            Purpose = "Auth",
            CreatedAtUtc = nowUtc,
            ExpiresAtUtc = nowUtc.AddMinutes(5),
            IsUsed = false,
            AttemptCount = 0
        };

        _context.UserOtps.Add(otp);
        await _context.SaveChangesAsync(cancellationToken);

        return new LoginOtpResult
        {
            UserId = userId.Value,
            EmployeeId = employee?.Id,
            Email = request.Email,
            OtpCode = otpCode
        };
    }

    private static void ThrowInvalidCredentials()
    {
        var errors = new Dictionary<string, string[]>
        {
            ["credentials"] = new[] { "Invalid email or password." }
        };

        throw new ValidationException(errors);
    }
}
