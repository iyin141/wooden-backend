using WoodLounge.Application.Common.Security;

namespace WoodLounge.Infrastructure.Security;

/// <summary>BCrypt password hasher (spec §4: admins only).</summary>
public sealed class BCryptPasswordHasher : IPasswordHasher
{
    public string Hash(string password) => BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12);
    public bool Verify(string password, string hash)
    {
        try { return BCrypt.Net.BCrypt.Verify(password, hash); }
        catch { return false; }
    }
}
