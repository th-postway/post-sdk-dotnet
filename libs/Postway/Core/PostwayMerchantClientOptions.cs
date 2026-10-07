namespace Postway;

/// <summary>Options for <see cref="PostwayMerchantClient"/>.</summary>
public sealed class PostwayMerchantClientOptions
{
    /// <summary>
    /// Merchant session access token, issued to you by Postway. Required for every call except
    /// <c>Receipts.*</c> and <c>Health.PingAsync()</c>.
    /// </summary>
    public string? AccessToken { get; set; }

    /// <summary>Token type sent before the token in <c>Authorization</c>. Default <c>"Bearer"</c>.</summary>
    public string TokenType { get; set; } = "Bearer";

    /// <summary>
    /// Full API base URL; overrides <see cref="Environment"/>. Must be <c>https://</c> (plain <c>http://</c> is
    /// accepted only for localhost) with no credentials, query string or fragment.
    /// </summary>
    public string? BaseUrl { get; set; }

    /// <summary>Named Postway environment. Default <see cref="MerchantEnvironment.Production"/>. Ignored when <see cref="BaseUrl"/> is set.</summary>
    public MerchantEnvironment Environment { get; set; } = MerchantEnvironment.Production;

    /// <summary>Per-request timeout. Default 60 seconds; a positive whole number of milliseconds, at most <see cref="int.MaxValue"/> ms.</summary>
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(60);

    /// <summary>
    /// <see cref="System.Net.Http.HttpClient"/> to send requests with (proxies, tracing, <c>IHttpClientFactory</c>, tests).
    /// The SDK never disposes it. Its handler must not follow redirects; a redirect the SDK sees is refused.
    /// Default: an SDK-owned client with redirects disabled, disposed with the <see cref="PostwayMerchantClient"/>.
    /// </summary>
    public HttpClient? HttpClient { get; set; }

    /// <summary><c>User-Agent</c> header. Default <c>postway-sdk-dotnet/&lt;version&gt; dotnet/&lt;runtime version&gt;</c>.</summary>
    public string? UserAgent { get; set; }
}
