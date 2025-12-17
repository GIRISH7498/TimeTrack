using TimeTrack.Application.Common.Models;

namespace TimeTrack.Application.Common.Interfaces
{
    public interface IJwtTokenService
    {
        Task<JwtTokenResult> GenerateTokenAsync(
            int userId,
            string email,
            CancellationToken cancellationToken);
    }
}
