namespace TimeTrack.Application.Common.Interfaces
{
    public interface ICurrentUserService
    {
        /// <summary>
        /// Returns the current authenticated user's Id (from JWT), or null if not authenticated.
        /// </summary>
        int? UserId { get; }

        /// <summary>
        /// Returns the current authenticated user's email, or null if not authenticated.
        /// </summary>
        string? Email { get; }

        /// <summary>
        /// True if there is an authenticated user on the current request.
        /// </summary>
        bool IsAuthenticated { get; }
    }
}
