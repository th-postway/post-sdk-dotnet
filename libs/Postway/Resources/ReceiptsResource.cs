using Postway.Models;

namespace Postway.Resources;

/// <summary>
/// <c>receipt/*</c> — the public receipt behind the QR code printed on receipts.
/// No access token is sent; the receipt token in the URL is the credential.
/// </summary>
public sealed class ReceiptsResource
{
    private readonly HttpPipeline _http;

    internal ReceiptsResource(HttpPipeline http) => _http = http;

    /// <summary>Receipt data as JSON. <c>GET receipt/public/:token</c>.</summary>
    /// <exception cref="PostwayApiException">404 when the token is invalid or the receipt is gone.</exception>
    public async Task<PublicReceiptResponse> GetPublicAsync(
        string token,
        RequestOptions? options = null,
        CancellationToken cancellationToken = default) =>
        await _http.RequestAsync<PublicReceiptResponse>(
            new HttpCall(HttpMethod.Get, ["receipt", "public", PathSegment.Param("token", token)]) { Options = options },
            cancellationToken).ConfigureAwait(false) ?? new PublicReceiptResponse();

    /// <summary>The rendered public receipt page. <c>GET receipt/:token</c>.</summary>
    /// <exception cref="PostwayApiException">404 when the token is invalid; its <c>Body</c> holds the "not found" page.</exception>
    public Task<string> GetPublicHtmlAsync(
        string token,
        RequestOptions? options = null,
        CancellationToken cancellationToken = default) =>
        _http.RequestTextAsync(
            new HttpCall(HttpMethod.Get, ["receipt", PathSegment.Param("token", token)]) { Options = options },
            cancellationToken);

    /// <summary>The URL of the public receipt page, e.g. to show or encode as a QR code. Makes no request.</summary>
    public string PublicUrl(string token) => _http.Url(["receipt", PathSegment.Param("token", token)]);
}
