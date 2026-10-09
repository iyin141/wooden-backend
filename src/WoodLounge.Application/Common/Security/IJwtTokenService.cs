namespace WoodLounge.Application.Common.Security;

public interface IJwtTokenService
{
    string GenerateAccessToken(Guid userId, string role, string? email = null, string? phone = null);
    string GenerateRefreshToken();
    string? GetUserIdFromToken(string token);
}
