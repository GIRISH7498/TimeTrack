using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TimeTrack.Application.Common.Exceptions;
using TimeTrack.Application.Common.Interfaces;
using TimeTrack.Infrastructure.Identity;

namespace TimeTrack.Infrastructure.Services
{
    public class IdentityService : IIdentityService
    {
        private readonly UserManager<AppUser> _userManager;

        public IdentityService(UserManager<AppUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<bool> CheckPasswordAsync(
            string email,
            string password,
            CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user is null || !user.IsActive)
            {
                return false;
            }

            return await _userManager.CheckPasswordAsync(user, password);
        }

        public async Task<int> CreateUserAsync(
            string email,
            string password,
            string? firstName,
            string? lastName,
            CancellationToken cancellationToken)
        {
            var existingUser = await _userManager.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);

            if (existingUser is not null)
            {
                var errors = new Dictionary<string, string[]>
                {
                    ["email"] = ["A user with this email already exists."]
                };

                throw new ValidationException(errors);
            }

            var user = new AppUser
            {
                UserName = email,
                Email = email,
                FirstName = firstName,
                LastName = lastName,
                IsActive = true
            };

            var result = await _userManager.CreateAsync(user, password);

            if (!result.Succeeded)
            {
                var errors = result.Errors
                    .GroupBy(e => e.Code)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(e => e.Description).ToArray()
                    );

                throw new ValidationException(errors);
            }

            return user.Id;
        }

        public async Task<int?> GetUserIdByEmailAsync(
            string email,
            CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByEmailAsync(email);
            return user?.Id;
        }

        public async Task ActivateUserAsync(
        int userId,
        CancellationToken cancellationToken)
        {
            var user = await _userManager.Users
                .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

            if (user is null)
            {
                throw new NotFoundException($"Attempted to activate non-existent user with Id {userId}");
            }

            var changed = false;

            if (!user.IsActive)
            {
                user.IsActive = true;
                changed = true;
            }

            if (!user.EmailConfirmed)
            {
                user.EmailConfirmed = true;
                changed = true;
            }

            if (changed)
            {
                await _userManager.UpdateAsync(user);
            }
        }

        public async Task ResetPasswordAsync(
            int userId,
            string newPassword,
            CancellationToken cancellationToken)
        {
            var user = await _userManager.Users
                .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

            if (user is null)
            {
                var errors = new Dictionary<string, string[]>
                {
                    ["user"] = new[] { "User not found." }
                };

                throw new ValidationException(errors);
            }

            var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, resetToken, newPassword);

            if (!result.Succeeded)
            {
                var errors = result.Errors
                    .GroupBy(e => e.Code)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(e => e.Description).Distinct().ToArray());

                throw new ValidationException(errors);
            }
        }
    }
}
