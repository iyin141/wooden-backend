using System.Text;

namespace WoodLounge.Infrastructure.Persistence;

/// <summary>
/// Converts a libpq-style connection URI (postgres://user:pass@host:port/db?params),
/// which is what Neon and most PaaS providers hand out, into the keyword form that
/// Npgsql's connection-string parser expects ("Host=…;Port=…;…"). Non-URI strings
/// (already in keyword form) are returned untouched.
/// </summary>
public static class ConnectionStringNormalizer
{
    // libpq query parameter -> Npgsql keyword. Unlisted parameters are appended verbatim.
    private static readonly Dictionary<string, string> ParamMap = new(StringComparer.OrdinalIgnoreCase)
    {
        ["sslmode"] = "SSL Mode",
        ["sslcert"] = "SSL Certificate",
        ["sslkey"] = "SSL Key",
        ["sslrootcert"] = "SSL Root Certificate",
        ["channel_binding"] = "Channel Binding",
        ["application_name"] = "Application Name",
        ["search_path"] = "Search Path",
        ["options"] = "Options",
        ["timeout"] = "Timeout",
        ["connect_timeout"] = "Connection Timeout",
        ["pooling"] = "Pooling",
        ["min_pool_size"] = "Minimum Pool Size",
        ["max_pool_size"] = "Maximum Pool Size",
    };

    public static string Normalize(string? connection)
    {
        if (string.IsNullOrWhiteSpace(connection))
            return string.Empty;

        // Already keyword form (no scheme) — pass through.
        if (!connection.Contains("://", StringComparison.Ordinal))
            return connection;

        var uri = new Uri(connection);

        var sb = new StringBuilder();
        void Add(string key, string? value)
        {
            if (string.IsNullOrEmpty(value)) return;
            if (sb.Length > 0) sb.Append(';');
            sb.Append(key).Append('=').Append(value);
        }

        Add("Host", uri.Host);
        if (!uri.IsDefaultPort) Add("Port", uri.Port.ToString());

        var db = uri.AbsolutePath.TrimStart('/');
        Add("Database", string.IsNullOrEmpty(db) ? null : Uri.UnescapeDataString(db));

        if (!string.IsNullOrEmpty(uri.UserInfo))
        {
            var parts = uri.UserInfo.Split(':', 2);
            Add("Username", Uri.UnescapeDataString(parts[0]));
            if (parts.Length == 2) Add("Password", Uri.UnescapeDataString(parts[1]));
        }

        foreach (var pair in ParseQuery(uri.Query))
        {
            var key = ParamMap.TryGetValue(pair.Key, out var mapped) ? mapped : pair.Key;
            Add(key, Uri.UnescapeDataString(pair.Value));
        }

        return sb.ToString();
    }

    private static IEnumerable<KeyValuePair<string, string>> ParseQuery(string query)
    {
        if (string.IsNullOrEmpty(query)) yield break;
        var q = query.TrimStart('?');
        foreach (var segment in q.Split('&', StringSplitOptions.RemoveEmptyEntries))
        {
            var kv = segment.Split('=', 2);
            yield return new KeyValuePair<string, string>(kv[0], kv.Length == 2 ? kv[1] : string.Empty);
        }
    }
}
