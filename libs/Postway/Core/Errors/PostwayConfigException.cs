namespace Postway;

/// <summary>
/// Invalid client options or call arguments (unsafe base URL, header value, timeout or path
/// parameter), or a guarded call without an access token. Messages never echo the offending value.
/// </summary>
public sealed class PostwayConfigException : PostwayException
{
    /// <summary>Creates the exception.</summary>
    public PostwayConfigException(string message)
        : base(message) { }
}
