namespace Postway;

/// <summary>
/// The server answered 2xx but its envelope says <c>isSuccess: false</c>. <c>order-shipment/create</c> and
/// <c>cancel</c> report courier/verification failures this way (with HTTP 201), so the SDK turns them
/// into an exception rather than a successful result.
/// </summary>
public sealed class PostwayBusinessException : PostwayApiException
{
    /// <summary>Creates the exception.</summary>
    public PostwayBusinessException(
        string method,
        string url,
        int status,
        int? code,
        IReadOnlyList<string> messages,
        string? body)
        : base(method, url, status, code, messages, body) { }
}
