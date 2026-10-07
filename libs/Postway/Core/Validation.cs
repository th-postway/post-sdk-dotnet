using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace Postway;

/// <summary>Input validation. Messages never include the offending value, so they are safe to log.</summary>
internal static partial class Validation
{
    /// <summary>Largest delay <see cref="CancellationTokenSource.CancelAfter(TimeSpan)"/> accepts (2^31 - 1 ms).</summary>
    public const long MaxTimeoutMs = int.MaxValue;

    private static readonly HashSet<string> LoopbackHosts = new(StringComparer.OrdinalIgnoreCase)
    {
        "localhost",
        "127.0.0.1",
        "[::1]",
    };

    /// <summary>
    /// Non-empty printable ASCII: what every HTTP stack accepts as a header value without complaint.
    /// <c>\z</c>, not <c>$</c>: in .NET <c>$</c> also matches before a trailing newline.
    /// </summary>
    [GeneratedRegex(@"^[\x20-\x7E]+\z")]
    private static partial Regex HeaderValue();

    /// <summary>RFC 9110 <c>token</c>: the auth-scheme part of <c>Authorization</c>.</summary>
    [GeneratedRegex(@"^[A-Za-z0-9!#$%&'*+.^_`|~-]+\z")]
    private static partial Regex AuthScheme();

    /// <summary>
    /// Validate and normalise a base URL: absolute, <c>https</c> (or <c>http</c> to a loopback host), no
    /// credentials, query or fragment. Returns the canonical form without trailing slashes.
    /// </summary>
    public static string NormalizeBaseUrl(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw) || !Uri.TryCreate(raw, UriKind.Absolute, out var uri))
        {
            throw new PostwayConfigException("BaseUrl must be an absolute URL");
        }

        var loopbackHttp = uri.Scheme == Uri.UriSchemeHttp && LoopbackHosts.Contains(uri.Host);
        if (uri.Scheme != Uri.UriSchemeHttps && !loopbackHttp)
        {
            throw new PostwayConfigException("BaseUrl must use https:// (http:// is only allowed for localhost)");
        }

        if (uri.UserInfo.Length > 0)
        {
            throw new PostwayConfigException("BaseUrl must not contain credentials");
        }

        if (uri.Query.Length > 0 || uri.Fragment.Length > 0 || raw.Contains('?') || raw.Contains('#'))
        {
            throw new PostwayConfigException("BaseUrl must not contain a query string or fragment");
        }

        return uri.GetComponents(UriComponents.SchemeAndServer | UriComponents.Path, UriFormat.UriEscaped)
            .TrimEnd('/');
    }

    /// <summary>A positive whole number of milliseconds that <see cref="CancellationTokenSource.CancelAfter(TimeSpan)"/> accepts.</summary>
    public static void AssertTimeout(TimeSpan value, string name)
    {
        if (value <= TimeSpan.Zero
            || value.Ticks % TimeSpan.TicksPerMillisecond != 0
            || value.Ticks / TimeSpan.TicksPerMillisecond > MaxTimeoutMs)
        {
            throw new PostwayConfigException(
                $"{name} must be a positive whole number of milliseconds (at most {MaxTimeoutMs} ms)");
        }
    }

    /// <summary>A value that can go on the wire as an HTTP header without being rejected or injecting headers.</summary>
    public static void AssertHeaderValue([NotNull] string? value, string name)
    {
        if (value is null || !HeaderValue().IsMatch(value))
        {
            throw new PostwayConfigException(
                $"{name} must be a non-empty string of printable ASCII characters (no line breaks or control characters)");
        }
    }

    /// <summary>The scheme word of <c>Authorization</c>, e.g. <c>Bearer</c>.</summary>
    public static void AssertAuthScheme([NotNull] string? value)
    {
        if (value is null || !AuthScheme().IsMatch(value))
        {
            throw new PostwayConfigException(
                "TokenType must be a single HTTP authentication scheme token such as \"Bearer\"");
        }
    }

    /// <summary>A path segment that cannot change which route the request reaches.</summary>
    public static void AssertPathSegment([NotNull] string? value, string name)
    {
        if (string.IsNullOrEmpty(value) || value == "." || value == "..")
        {
            throw new PostwayConfigException($"{name} must be a non-empty string other than \".\" or \"..\"");
        }
    }
}
