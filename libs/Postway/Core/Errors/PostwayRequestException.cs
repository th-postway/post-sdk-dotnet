namespace Postway;

/// <summary>The request did not complete: network failure, refused redirect, or timeout.</summary>
public sealed class PostwayRequestException : PostwayException
{
    /// <summary>Creates the exception.</summary>
    public PostwayRequestException(string method, string url, string message, Exception? innerException = null)
        : base(message, innerException)
    {
        Method = method;
        Url = url;
    }

    /// <summary>HTTP method of the request.</summary>
    public string Method { get; }

    /// <summary>Request URL with caller-supplied path parameters replaced by <c>:name</c> placeholders.</summary>
    public string Url { get; }
}
