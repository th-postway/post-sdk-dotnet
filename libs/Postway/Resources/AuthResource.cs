using Postway.Models;

namespace Postway.Resources;

/// <summary><c>auth/*</c> — session introspection.</summary>
public sealed class AuthResource
{
    private readonly HttpPipeline _http;

    internal AuthResource(HttpPipeline http) => _http = http;

    /// <summary>The store, owner and session expiry behind the access token. <c>POST auth/account/info</c>.</summary>
    public async Task<MerchantAuthAccountInfoResponse> AccountInfoAsync(
        RequestOptions? options = null,
        CancellationToken cancellationToken = default) =>
        await _http.RequestAsync<MerchantAuthAccountInfoResponse>(
            new HttpCall(HttpMethod.Post, ["auth", "account", "info"]) { Auth = true, ObservesSession = true, Options = options },
            cancellationToken).ConfigureAwait(false) ?? new MerchantAuthAccountInfoResponse();
}
