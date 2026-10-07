namespace Postway.Resources;

/// <summary><c>health/*</c> — liveness.</summary>
public sealed class HealthResource
{
    private readonly HttpPipeline _http;

    internal HealthResource(HttpPipeline http) => _http = http;

    /// <summary>Returns <c>"pong"</c> when the Merchant API is reachable. <c>GET health/ping</c>, no auth.</summary>
    public Task<string> PingAsync(RequestOptions? options = null, CancellationToken cancellationToken = default) =>
        _http.RequestTextAsync(new HttpCall(HttpMethod.Get, ["health", "ping"]) { Options = options }, cancellationToken);
}
