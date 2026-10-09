using Microsoft.EntityFrameworkCore;
using WoodLounge.Application.Auth;
using WoodLounge.Domain.Identity;
using WoodLounge.Infrastructure.Persistence;

namespace WoodLounge.Infrastructure.Auth;

public class AuthRepository(WoodLoungeDbContext db) : Application.Auth.IAuthRepository
{
    public async Task<User?> FindByPhoneAsync(string phone, CancellationToken ct = default)
    {
        return await db.Users.FirstOrDefaultAsync(u => u.Phone == phone, ct);
    }

    public async Task<User?> FindByEmailAsync(string email, CancellationToken ct = default)
    {
        return await db.Users.FirstOrDefaultAsync(u => u.Email == email, ct);
    }

    public async Task<User?> FindByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await db.Users.FindAsync(id, ct).AsTask();
    }

    public async Task AddUserAsync(User user, CancellationToken ct = default)
    {
        db.Users.Add(user);
        await db.SaveChangesAsync(ct);
    }

    public async Task UpdateUserAsync(User user, CancellationToken ct = default)
    {
        db.Users.Update(user);
        await db.SaveChangesAsync(ct);
    }

    public async Task AddOtpCodeAsync(OtpCode otpCode, CancellationToken ct = default)
    {
        db.OtpCodes.Add(otpCode);
        await db.SaveChangesAsync(ct);
    }

    public async Task<OtpCode?> FindValidOtpAsync(string phone, string code, CancellationToken ct = default)
    {
        return await db.OtpCodes
            .Where(o => o.Phone == phone && o.ExpiresAt > DateTimeOffset.UtcNow && o.ConsumedAt == null)
            .OrderByDescending(o => o.CreatedAt)
            .FirstOrDefaultAsync(ct);
    }

    public async Task AddRefreshTokenAsync(RefreshToken refreshToken, CancellationToken ct = default)
    {
        db.RefreshTokens.Add(refreshToken);
        await db.SaveChangesAsync(ct);
    }

    public async Task<RefreshToken?> FindRefreshTokenAsync(Guid userId, string tokenHash, CancellationToken ct = default)
    {
        return await db.RefreshTokens
            .FirstOrDefaultAsync(t => t.UserId == userId && t.TokenHash == tokenHash && t.RevokedAt == null && t.ExpiresAt > DateTimeOffset.UtcNow, ct);
    }

    public async Task RevokeRefreshTokenAsync(Guid userId, CancellationToken ct = default)
    {
        var tokens = await db.RefreshTokens
            .Where(t => t.UserId == userId && t.RevokedAt == null)
            .ToListAsync(ct);

        foreach (var token in tokens)
        {
            token.RevokedAt = DateTimeOffset.UtcNow;
        }

        await db.SaveChangesAsync(ct);
    }
}
