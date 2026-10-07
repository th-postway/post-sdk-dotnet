namespace Postway;

/// <summary>Describes one HTTP call relative to the base URL.</summary>
internal sealed record HttpCall(HttpMethod Method, PathSegment[] Path)
{
    public IReadOnlyList<KeyValuePair<string, string?>>? Query { get; init; }

    /// <summary>JSON body; <c>null</c> sends no body and no <c>Content-Type</c>.</summary>
    public object? Body { get; init; }

    /// <summary>Send <c>Authorization</c> (authenticated routes).</summary>
    public bool Auth { get; init; }

    public RequestOptions? Options { get; init; }
}
