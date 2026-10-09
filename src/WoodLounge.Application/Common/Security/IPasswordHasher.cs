namespace WoodLounge.Application.Common.Security;

/// <summary>
/// Password hashing for admin accounts (spec §4: Argon2id/BCrypt). Customers use OTP/Google.
/// </summary>
public interface IPasswordHasher
{
    string Hash(string password);
    bool Verify(string password, string hash);
}
