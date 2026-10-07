namespace Postway;

/// <summary>Per-call options accepted by every SDK method.</summary>
public sealed class RequestOptions
{
    /// <summary>Overrides the client's <see cref="PostwayMerchantClientOptions.Timeout"/> for this call.</summary>
    public TimeSpan? Timeout { get; init; }
}
