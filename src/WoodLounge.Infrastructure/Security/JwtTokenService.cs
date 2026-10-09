using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using WoodLounge.Application.Common.Security;

namespace WoodLounge.Infrastructure.Security;

public class JwtTokenService(IConfiguration config) : IJwtTokenService
{
    private readonly string _signingKey = config["JWT_SIGNING_KEY"]
        ?? throw new InvalidOperationException("JWT_SIGNING_KEY is not configured");
    private readonly string _issuer = config["JWT_ISSUER"] ?? "wood-lounge-api";
    private readonly string _audience = config["JWT_AUDIENCE"] ?? "wood-lounge-web";
    private readonly int _accessTokenExpirationMinutes = 15;
    private readonly int _refreshTokenExpirationDays = 30;

    public string GenerateAccessToken(Guid userId, string role, string? email = null, string? phone = null)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(ClaimTypes.NameIdentifier, userId.ToString()),
            new(ClaimTypes.Role, role)
        };

        if (!string.IsNullOrWhiteSpace(email))
            claims.Add(new(ClaimTypes.Email, email));

        if (!string.IsNullOrWhiteSpace(phone))
            claims.Add(new("phone", phone));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_signingKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _issuer,
            audience: _audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_accessTokenExpirationMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string GenerateRefreshToken()
    {
        var randomBytes = new byte[64];
        using var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }

    public string? GetUserIdFromToken(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_signingKey);

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = _issuer,
                ValidAudience = _audience,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ClockSkew = TimeSpan.Zero
            };

            var principal = tokenHandler.ValidateToken(token, validationParameters, out _);
            var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(userIdClaim, out var userId) ? userId.ToString() : null;
        }
        catch
        {
            return null;
        }
    }
}
