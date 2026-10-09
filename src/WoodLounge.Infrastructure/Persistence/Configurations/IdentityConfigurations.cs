using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WoodLounge.Domain.Identity;

namespace WoodLounge.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> b)
    {
        b.ToTable("users", t =>
        {
            t.HasCheckConstraint("ck_users_role", "role in ('customer','admin','super_admin')");
            t.HasCheckConstraint("ck_users_phone_or_email", "phone is not null or email is not null");
        });
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasDefaultValueSql("gen_random_uuid()");
        b.Property(x => x.Phone).HasColumnType("text");
        b.HasIndex(x => x.Phone).IsUnique();
        b.Property(x => x.Email).HasColumnType("citext");
        b.HasIndex(x => x.Email).IsUnique();
        b.Property(x => x.FullName).HasColumnType("text");
        b.Property(x => x.Role).HasColumnType("text").IsRequired().HasDefaultValue("customer");
        b.Property(x => x.PasswordHash).HasColumnType("text");
        b.Property(x => x.GoogleSub).HasColumnType("text");
        b.HasIndex(x => x.GoogleSub).IsUnique();
        b.Property(x => x.IsActive).HasDefaultValue(true);
        b.Property(x => x.CreatedAt).HasDefaultValueSql("now()");
        b.Property(x => x.UpdatedAt).HasDefaultValueSql("now()");
    }
}

public class OtpCodeConfiguration : IEntityTypeConfiguration<OtpCode>
{
    public void Configure(EntityTypeBuilder<OtpCode> b)
    {
        b.ToTable("otp_codes", t =>
            t.HasCheckConstraint("ck_otp_purpose", "purpose in ('login','verify_phone')"));
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasDefaultValueSql("gen_random_uuid()");
        b.Property(x => x.Phone).HasColumnType("text").IsRequired();
        b.Property(x => x.CodeHash).HasColumnType("text").IsRequired();
        b.Property(x => x.Purpose).HasColumnType("text").IsRequired().HasDefaultValue("login");
        b.Property(x => x.Attempts).HasDefaultValue(0);
        b.Property(x => x.Ip).HasColumnType("inet");
        b.Property(x => x.CreatedAt).HasDefaultValueSql("now()");
        b.HasIndex(x => new { x.Phone, x.CreatedAt }).IsDescending(false, true)
            .HasDatabaseName("ix_otp_phone_created");
    }
}

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> b)
    {
        b.ToTable("refresh_tokens");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasDefaultValueSql("gen_random_uuid()");
        b.Property(x => x.TokenHash).HasColumnType("text").IsRequired();
        b.HasIndex(x => x.TokenHash).IsUnique();
        b.Property(x => x.UserAgent).HasColumnType("text");
        b.Property(x => x.Ip).HasColumnType("inet");
        b.Property(x => x.CreatedAt).HasDefaultValueSql("now()");
        b.HasIndex(x => x.UserId).HasDatabaseName("ix_refresh_user");
        b.HasOne(x => x.User).WithMany(u => u.RefreshTokens).HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
        b.HasOne(x => x.ReplacedBy).WithMany().HasForeignKey(x => x.ReplacedById).OnDelete(DeleteBehavior.NoAction);
    }
}

public class AddressConfiguration : IEntityTypeConfiguration<Address>
{
    public void Configure(EntityTypeBuilder<Address> b)
    {
        b.ToTable("addresses");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasDefaultValueSql("gen_random_uuid()");
        b.Property(x => x.Label).HasColumnType("text");
        b.Property(x => x.Recipient).HasColumnType("text").IsRequired();
        b.Property(x => x.Phone).HasColumnType("text").IsRequired();
        b.Property(x => x.Line1).HasColumnType("text").IsRequired();
        b.Property(x => x.Line2).HasColumnType("text");
        b.Property(x => x.City).HasColumnType("text").IsRequired();
        b.Property(x => x.State).HasColumnType("text").IsRequired();
        b.Property(x => x.Landmark).HasColumnType("text");
        b.Property(x => x.IsDefault).HasDefaultValue(false);
        b.Property(x => x.CreatedAt).HasDefaultValueSql("now()");
        b.Property(x => x.UpdatedAt).HasDefaultValueSql("now()");
        b.HasIndex(x => x.UserId).HasDatabaseName("ix_addresses_user");
        b.HasOne(x => x.User).WithMany(u => u.Addresses).HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
    }
}
