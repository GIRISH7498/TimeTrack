using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using TimeTrack.Application.Common.Interfaces;
using TimeTrack.Application.Common.Models;

namespace TimeTrack.Infrastructure.Services;

public class JwtTokenService : IJwtTokenService
{
    private readonly IConfiguration _configuration;

    public JwtTokenService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public Task<JwtTokenResult> GenerateTokenAsync(
        int userId,
        string email,
        CancellationToken cancellationToken)
    {
        var jwtSection = _configuration.GetSection("Jwt");

        var key = jwtSection["Key"]
                  ?? throw new InvalidOperationException("Jwt:Key is not configured.");

        var issuer = jwtSection["Issuer"];
        var audience = jwtSection["Audience"];

        var expiresInMinutes = int.TryParse(jwtSection["ExpiresInMinutes"], out var minutes)
            ? minutes
            : 60;

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new(JwtRegisteredClaimNames.Email, email),
            new(ClaimTypes.NameIdentifier, userId.ToString())
        };

        var expiresAtUtc = DateTime.UtcNow.AddMinutes(expiresInMinutes);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: expiresAtUtc,
            signingCredentials: credentials);

        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

        var result = new JwtTokenResult
        {
            Token = tokenString,
            ExpiresAtUtc = expiresAtUtc
        };

        return Task.FromResult(result);
    }
}
