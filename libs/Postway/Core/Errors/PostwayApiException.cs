using System.Text.Json.Serialization;

namespace Postway;

/// <summary>
/// The server answered with a non-2xx status. Error bodies have the shape
/// <c>{ code, isSuccess: false, message, data: null }</c>.
/// </summary>
/// <remarks>
/// Typical statuses: 400 (validation / business rule), 403 (missing, unknown or expired token),
/// 404 (public receipt not found), 500 (server error).
/// </remarks>
public class PostwayApiException : PostwayException
{
    /// <summary>Creates the exception. The message defaults to the server messages joined with <c>"; "</c>, or <c>HTTP {status}</c>.</summary>
    public PostwayApiException(
        string method,
        string url,
        int status,
        int? code,
        IReadOnlyList<string> messages,
        string? body,
        string? message = null,
        Exception? innerException = null)
        : base(message ?? DefaultMessage(status, messages), innerException)
    {
        Method = method;
        Url = url;
        Status = status;
        Code = code;
        Messages = messages;
        Body = body;
    }

    /// <summary>HTTP method of the request.</summary>
    public string Method { get; }

    /// <summary>Request URL with caller-supplied path parameters replaced by <c>:name</c> placeholders.</summary>
    public string Url { get; }

    /// <summary>HTTP status of the response.</summary>
    public int Status { get; }

    /// <summary>Body <c>code</c> from the server envelope, when present (400 or 500 on errors). Not the HTTP status.</summary>
    public int? Code { get; }

    /// <summary>Server messages; validation failures return several.</summary>
    public IReadOnlyList<string> Messages { get; }

    /// <summary>
    /// Raw response body text, or <c>null</c> when empty. Never part of <see cref="Exception.Message"/>,
    /// <see cref="Exception.ToString"/> or JSON serialization; read it explicitly when you need it.
    /// </summary>
    [JsonIgnore]
    public string? Body { get; }

    private static string DefaultMessage(int status, IReadOnlyList<string> messages) =>
        messages.Count > 0 ? string.Join("; ", messages) : $"HTTP {status}";
}
