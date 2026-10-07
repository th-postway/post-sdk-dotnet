namespace Postway;

/// <summary>Base class for every exception thrown by this SDK.</summary>
public class PostwayException : Exception
{
    /// <summary>Creates the exception.</summary>
    public PostwayException(string message, Exception? innerException = null)
        : base(message, innerException) { }
}
