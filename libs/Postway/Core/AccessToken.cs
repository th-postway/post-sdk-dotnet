namespace Postway;

/// <summary>A merchant access token from <see cref="PostwayMerchantClientOptions.AccessTokenProvider"/>.</summary>
/// <param name="Value">The token sent after the token type in <c>Authorization</c>.</param>
/// <param name="ExpiresAt">
/// When the token stops being accepted. When <c>null</c> the SDK reads the JWT <c>exp</c> claim, or asks
/// <c>auth/account/info</c> once for <c>session.expired</c>.
/// </param>
public sealed record AccessToken(string Value, DateTimeOffset? ExpiresAt = null)
{
    /// <summary>Keeps the token out of logs.</summary>
    public override string ToString() => ExpiresAt is { } expiresAt ? $"AccessToken {{ ExpiresAt = {expiresAt:O} }}" : "AccessToken";
}

/// <summary>Why the SDK asks <see cref="PostwayMerchantClientOptions.AccessTokenProvider"/> for a token.</summary>
public enum AccessTokenRefreshReason
{
    /// <summary>The client has no token yet.</summary>
    Initial,

    /// <summary>At least 75% of the current token's lifetime has elapsed.</summary>
    Expiring,

    /// <summary>The API answered 403 to the current token.</summary>
    Forbidden,
}
