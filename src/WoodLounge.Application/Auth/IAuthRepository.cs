using WoodLounge.Domain.Identity;

namespace WoodLounge.Application.Auth;

public interface IAuthRepository
{
    Task<User?> FindByPhoneAsync(string phone, CancellationToken ct = default);
    Task<User?> FindByEmailAsync(string email, CancellationToken ct = default);
    Task<User?> FindByIdAsync(Guid id, CancellationToken ct = default);
    Task AddUserAsync(User user, CancellationToken ct = default);
    Task UpdateUserAsync(User user, CancellationToken ct = default);
    Task AddOtpCodeAsync(OtpCode otpCode, CancellationToken ct = default);
    Task<OtpCode?> FindValidOtpAsync(string phone, string code, CancellationToken ct = default);
    Task AddRefreshTokenAsync(RefreshToken refreshToken, CancellationToken ct = default);
    Task<RefreshToken?> FindRefreshTokenAsync(Guid userId, string tokenHash, CancellationToken ct = default);
    Task RevokeRefreshTokenAsync(Guid userId, CancellationToken ct = default);
}
