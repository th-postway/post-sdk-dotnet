namespace Postway;

/// <summary>
/// One path segment of a call. Caller input goes through <see cref="Param"/> so it is validated and shows
/// as <c>:name</c> in error URLs and messages; <see cref="Static"/> is for route literals.
/// </summary>
internal readonly record struct PathSegment(string? Name, string? Value)
{
    public static PathSegment Static(string value) => new(null, value);

    public static PathSegment Param(string name, string? value) => new(name, value);

    public static implicit operator PathSegment(string value) => Static(value);
}
