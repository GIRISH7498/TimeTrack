namespace TimeTrack.Application.Common.Interfaces
{
    public interface IIdentityService
    {
        /// <summary>
        /// Creates a new user in the Users table and returns its Id.
        /// Should throw ValidationException if the user cannot be created
        /// (e.g. email already exists, password doesn't meet rules, etc.).
        /// </summary>
        Task<int> CreateUserAsync(
            string email,
            string password,
            string? firstName,
            string? lastName,
            CancellationToken cancellationToken);


        /// <summary>
        /// Checks if the given email/password pair is valid.
        /// </summary>
        Task<bool> CheckPasswordAsync(string email, string password, CancellationToken cancellationToken);

        /// <summary>
        /// Returns user Id by email, or null if not found.
        /// </summary>
        Task<int?> GetUserIdByEmailAsync(string email, CancellationToken cancellationToken);

        Task ActivateUserAsync(
            int userId,
            CancellationToken cancellationToken);

        Task ResetPasswordAsync(
            int userId,
            string newPassword,
            CancellationToken cancellationToken);
    }
}
