using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using TimeTrack.Application.Common.Interfaces;

namespace TimeTrack.API.Services
{
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public bool IsAuthenticated
            => _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated == true;

        public int? UserId
        {
            get
            {
                if (!IsAuthenticated)
                    return null;

                var user = _httpContextAccessor.HttpContext!.User;

                // Prefer NameIdentifier; fallback to "sub"
                var idValue = user.FindFirstValue(ClaimTypes.NameIdentifier)
                              ?? user.FindFirstValue(JwtRegisteredClaimNames.Sub);

                if (int.TryParse(idValue, out var id))
                {
                    return id;
                }

                return null;
            }
        }

        public string? Email
        {
            get
            {
                if (!IsAuthenticated)
                    return null;

                var user = _httpContextAccessor.HttpContext!.User;

                // Prefer JWT email, fallback to ClaimTypes.Email
                return user.FindFirstValue(JwtRegisteredClaimNames.Email)
                       ?? user.FindFirstValue(ClaimTypes.Email);
            }
        }
    }
}
