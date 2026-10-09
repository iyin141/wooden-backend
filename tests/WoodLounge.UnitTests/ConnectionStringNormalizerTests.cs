using WoodLounge.Infrastructure.Persistence;

namespace WoodLounge.UnitTests;

public class ConnectionStringNormalizerTests
{
    [Fact]
    public void Normalize_KeywordForm_ReturnedUnchanged()
    {
        const string keyword = "Host=localhost;Port=5432;Database=woodlounge;Username=u;Password=p";
        Assert.Equal(keyword, ConnectionStringNormalizer.Normalize(keyword));
    }

    [Fact]
    public void Normalize_NeonUri_ConvertsToKeywordForm()
    {
        var uri = "postgres://user:secret@ep-cool-1234.us-east-2.aws.neon.tech/neondb?sslmode=require";
        var result = ConnectionStringNormalizer.Normalize(uri);

        Assert.Contains("Host=ep-cool-1234.us-east-2.aws.neon.tech", result);
        Assert.Contains("Database=neondb", result);
        Assert.Contains("Username=user", result);
        Assert.Contains("Password=secret", result);
        Assert.Contains("SSL Mode=require", result);
    }

    [Fact]
    public void Normalize_UrlEncodedPassword_IsDecoded()
    {
        var result = ConnectionStringNormalizer.Normalize("postgresql://u:p%40ss%3Dword@localhost:5433/db");
        Assert.Contains("Password=p@ss=word", result);
        Assert.Contains("Port=5433", result);
    }

    [Fact]
    public void Normalize_NullOrEmpty_ReturnsEmpty()
    {
        Assert.Equal(string.Empty, ConnectionStringNormalizer.Normalize(null));
        Assert.Equal(string.Empty, ConnectionStringNormalizer.Normalize("  "));
    }
}
