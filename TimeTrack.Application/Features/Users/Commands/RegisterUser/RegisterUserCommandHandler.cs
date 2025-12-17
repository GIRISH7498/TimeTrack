using MediatR;
using TimeTrack.Application.Common.Interfaces;
using TimeTrack.Domain.Entities;

namespace TimeTrack.Application.Features.Users.Commands.RegisterUser
{
    public class RegisterUserCommandHandler
        : IRequestHandler<RegisterUserCommand, RegisterUserResult>
    {
        private readonly IIdentityService _identityService;
        private readonly IApplicationDbContext _context;

        public RegisterUserCommandHandler(IIdentityService identityService, IApplicationDbContext context)
        {
            _identityService = identityService;
            _context = context;
        }


        public async Task<RegisterUserResult> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
        {
            var userId = await _identityService.CreateUserAsync(
                request.Email,
                request.Password,
                request.FirstName,
                request.LastName,
                cancellationToken);

            var employee = new Employee
            {
                UserId = userId,
                EmployeeCode = request.EmployeeCode,
                FirstName = request.FirstName,
                LastName = request.LastName,
                Department = request.Department,
                Designation = request.Designation,
                JoiningDate = request.JoiningDate,
                IsActive = true
            };

            var nowUtc = DateTime.UtcNow;
            var otpCode = new Random().Next(100000, 999999).ToString();

            var otp = new UserOtp
            {
                UserId = userId,
                Code = otpCode,
                Purpose = "Auth",
                CreatedAtUtc = nowUtc,
                ExpiresAtUtc = nowUtc.AddMinutes(5),
                IsUsed = false,
                AttemptCount = 0
            };

            _context.Employees.Add(employee);
            _context.UserOtps.Add(otp);

            await _context.SaveChangesAsync(cancellationToken);

            return new RegisterUserResult
            {
                UserId = userId,
                EmployeeId = employee.Id,
                Email = request.Email,
                OtpCode = otpCode
            };
        }
    }
}
