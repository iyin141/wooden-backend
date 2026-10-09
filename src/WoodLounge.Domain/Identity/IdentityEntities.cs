using WoodLounge.Domain.Common;

namespace WoodLounge.Domain.Identity;
public class User : AuditedEntity
{
    public string? Phone { get; set; }              // E.164, e.g. +2348012345678
    public string? Email { get; set; }              // citext
    public string? FullName { get; set; }
    public string Role { get; set; } = "customer";  // customer | admin | super_admin
    public string? PasswordHash { get; set; }       // admins only
    public string? GoogleSub { get; set; }
    public DateTimeOffset? PhoneVerifiedAt { get; set; }
    public DateTimeOffset? EmailVerifiedAt { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTimeOffset? LastLoginAt { get; set; }

    public ICollection<Address> Addresses { get; set; } = new List<Address>();
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
}

public class OtpCode : Entity
{
    public string Phone { get; set; } = null!;
    public string CodeHash { get; set; } = null!;   // never store the raw code
    public string Purpose { get; set; } = "login";  // login | verify_phone
    public int Attempts { get; set; }
    public DateTimeOffset ExpiresAt { get; set; }
    public DateTimeOffset? ConsumedAt { get; set; }
    public System.Net.IPAddress? Ip { get; set; }   // inet
    public DateTimeOffset CreatedAt { get; set; }
}

public class RefreshToken : Entity
{
    public Guid UserId { get; set; }
    public User? User { get; set; }
    public string TokenHash { get; set; } = null!;
    public DateTimeOffset ExpiresAt { get; set; }
    public DateTimeOffset? RevokedAt { get; set; }
    public Guid? ReplacedById { get; set; }
    public RefreshToken? ReplacedBy { get; set; }
    public string? UserAgent { get; set; }
    public System.Net.IPAddress? Ip { get; set; }   // inet
    public DateTimeOffset CreatedAt { get; set; }
}

public class Address : AuditedEntity
{
    public Guid UserId { get; set; }
    public User? User { get; set; }
    public string? Label { get; set; }              // Home / Office
    public string Recipient { get; set; } = null!;
    public string Phone { get; set; } = null!;
    public string Line1 { get; set; } = null!;
    public string? Line2 { get; set; }
    public string City { get; set; } = null!;
    public string State { get; set; } = null!;      // one of the 36 states + FCT
    public string? Landmark { get; set; }
    public bool IsDefault { get; set; }
}
