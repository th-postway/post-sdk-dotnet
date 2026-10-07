using System.Text.Json;

namespace Postway;

/// <summary>Holds the current access token and refreshes it through the caller's provider.</summary>
internal sealed class AccessTokenManager : IDisposable
{
    /// <summary>Share of a token's lifetime after which it is refreshed before use.</summary>
    public const double RefreshAfterElapsed = 0.75;

    private readonly Func<AccessTokenRefreshReason, CancellationToken, ValueTask<AccessToken>>? _provider;
    private readonly TimeProvider _time;
    private readonly SemaphoreSlim _refreshLock = new(1, 1);
    private TokenState? _state;

    public AccessTokenManager(
        string? accessToken,
        Func<AccessTokenRefreshReason, CancellationToken, ValueTask<AccessToken>>? provider,
        TimeProvider time)
    {
        _provider = provider;
        _time = time;
        if (accessToken is not null)
        {
            _state = NewState(accessToken, null, time.GetUtcNow());
        }
    }

    /// <summary>A token is set or can be obtained.</summary>
    public bool Configured => Volatile.Read(ref _state) is not null || _provider is not null;

    /// <summary>Automatic refresh (and the single 403 replay) is on.</summary>
    public bool CanRefresh => _provider is not null;

    /// <summary>
    /// Token for an authenticated call. With a provider: obtains the first token, learns an unknown lifetime
    /// through <paramref name="probe"/> (once per token; a 403 there refreshes immediately) and refreshes once
    /// at least 75% of the lifetime has elapsed. <c>RefreshedAfterForbidden</c> reports that the call's one
    /// 403-triggered refresh is used up.
    /// </summary>
    public async Task<(string Token, bool RefreshedAfterForbidden)> ResolveAsync(
        Func<string, CancellationToken, Task<(int Status, string? Body)>>? probe,
        CancellationToken cancellationToken)
    {
        var state = Volatile.Read(ref _state)
            ?? await RefreshAsync(null, AccessTokenRefreshReason.Initial, cancellationToken).ConfigureAwait(false);
        if (_provider is null)
        {
            return (state.Value, false);
        }

        var refreshedAfterForbidden = false;
        if (!state.Settled && probe is not null && await ProbeAsync(state, probe, cancellationToken).ConfigureAwait(false))
        {
            state = await RefreshAsync(state, AccessTokenRefreshReason.Forbidden, cancellationToken).ConfigureAwait(false);
            refreshedAfterForbidden = true;
        }

        if (IsExpiring(state, _time.GetUtcNow()))
        {
            state = await RefreshAsync(state, AccessTokenRefreshReason.Expiring, cancellationToken).ConfigureAwait(false);
        }

        return (state.Value, refreshedAfterForbidden);
    }

    /// <summary>New token after <paramref name="sent"/> got a 403; reuses a newer token if another call already refreshed.</summary>
    public async Task<string> RefreshAfterForbiddenAsync(string sent, CancellationToken cancellationToken)
    {
        var state = Volatile.Read(ref _state);
        if (state is not null && state.Value != sent)
        {
            return state.Value;
        }

        return (await RefreshAsync(state, AccessTokenRefreshReason.Forbidden, cancellationToken).ConfigureAwait(false)).Value;
    }

    /// <summary>Record <c>session.expired</c> from an <c>auth/account/info</c> JSON body for <paramref name="sent"/>, if still current.</summary>
    public void ObserveSession(string sent, string? body)
    {
        var state = Volatile.Read(ref _state);
        if (state is null || state.Value != sent || ParseSessionExpired(body) is not { } expiresAt)
        {
            return;
        }

        state.Lifetime = new Lifetime(state.ObtainedAt, expiresAt);
        state.Settled = true;
    }

    public void Dispose() => _refreshLock.Dispose();

    /// <summary>
    /// <c>exp</c> / <c>iat</c> of a JWT, read without verifying the signature (they only schedule a refresh).
    /// <c>null</c> for anything that is not a JWT with a numeric <c>exp</c>.
    /// </summary>
    internal static Lifetime? DecodeJwt(string token, DateTimeOffset obtainedAt)
    {
        var parts = token.Split('.');
        if (parts.Length != 3 || parts[1].Length == 0)
        {
            return null;
        }

        var base64 = parts[1].Replace('-', '+').Replace('_', '/');
        base64 = base64.PadRight(base64.Length + ((4 - (base64.Length % 4)) % 4), '=');
        var bytes = new byte[base64.Length];
        if (!Convert.TryFromBase64String(base64, bytes, out var written))
        {
            return null;
        }

        try
        {
            using var document = JsonDocument.Parse(bytes.AsMemory(0, written));
            var root = document.RootElement;
            if (root.ValueKind != JsonValueKind.Object
                || !root.TryGetProperty("exp", out var expElement)
                || expElement.ValueKind != JsonValueKind.Number
                || !expElement.TryGetDouble(out var exp))
            {
                return null;
            }

            var expiresAt = DateTimeOffset.FromUnixTimeMilliseconds(checked((long)(exp * 1000)));
            var startsAt = root.TryGetProperty("iat", out var iatElement)
                && iatElement.ValueKind == JsonValueKind.Number
                && iatElement.TryGetDouble(out var iat)
                && iat < exp
                    ? DateTimeOffset.FromUnixTimeMilliseconds(checked((long)(iat * 1000)))
                    : obtainedAt;
            return new Lifetime(startsAt, expiresAt);
        }
        catch (Exception error) when (error is JsonException or ArgumentOutOfRangeException or OverflowException)
        {
            return null;
        }
    }

    private static DateTimeOffset? ParseSessionExpired(string? body)
    {
        if (body is null)
        {
            return null;
        }

        try
        {
            using var document = JsonDocument.Parse(body);
            return document.RootElement.ValueKind == JsonValueKind.Object
                && document.RootElement.TryGetProperty("session", out var session)
                && session.ValueKind == JsonValueKind.Object
                && session.TryGetProperty("expired", out var expired)
                && expired.ValueKind == JsonValueKind.String
                && expired.TryGetDateTimeOffset(out var value)
                && value != default
                    ? value
                    : null;
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private static bool IsExpiring(TokenState state, DateTimeOffset now) =>
        state.Lifetime is { } lifetime
        && now >= lifetime.StartsAt + ((lifetime.ExpiresAt - lifetime.StartsAt) * RefreshAfterElapsed);

    private static TokenState NewState(string value, DateTimeOffset? expiresAt, DateTimeOffset now) =>
        new(value, now, expiresAt is { } known ? new Lifetime(now, known) : DecodeJwt(value, now));

    /// <summary>Concurrent callers share one provider call; a caller that saw a stale state gets the newer one.</summary>
    private async Task<TokenState> RefreshAsync(TokenState? from, AccessTokenRefreshReason reason, CancellationToken cancellationToken)
    {
        var current = Volatile.Read(ref _state);
        if (current is not null && !ReferenceEquals(current, from))
        {
            return current;
        }

        if (_provider is null)
        {
            throw new PostwayConfigException("AccessTokenProvider is required to refresh the access token");
        }

        await _refreshLock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            current = Volatile.Read(ref _state);
            if (current is not null && !ReferenceEquals(current, from))
            {
                return current;
            }

            var token = await _provider(reason, cancellationToken).ConfigureAwait(false)
                ?? throw new PostwayConfigException("AccessTokenProvider must return an AccessToken");
            Validation.AssertHeaderValue(token.Value, "AccessTokenProvider result");
            var state = NewState(token.Value, token.ExpiresAt == default(DateTimeOffset) ? null : token.ExpiresAt, _time.GetUtcNow());
            Volatile.Write(ref _state, state);
            return state;
        }
        finally
        {
            _refreshLock.Release();
        }
    }

    /// <summary><c>true</c> when the probe got a 403. Any other failure just settles the token with an unknown lifetime.</summary>
    private Task<bool> ProbeAsync(
        TokenState state,
        Func<string, CancellationToken, Task<(int Status, string? Body)>> probe,
        CancellationToken cancellationToken)
    {
        var running = new Lazy<Task<bool>>(() => RunProbeAsync(state, probe, cancellationToken));
        return (Interlocked.CompareExchange(ref state.Probe, running, null) ?? running).Value;
    }

    private async Task<bool> RunProbeAsync(
        TokenState state,
        Func<string, CancellationToken, Task<(int Status, string? Body)>> probe,
        CancellationToken cancellationToken)
    {
        try
        {
            var (status, body) = await probe(state.Value, cancellationToken).ConfigureAwait(false);
            if (status is >= 200 and < 300)
            {
                ObserveSession(state.Value, body);
            }

            return status == 403;
        }
#pragma warning disable CA1031 // the probe only informs scheduling; the real call reports its own failures
        catch (Exception)
#pragma warning restore CA1031
        {
            return false;
        }
        finally
        {
            state.Settled = true;
        }
    }

    internal sealed record Lifetime(DateTimeOffset StartsAt, DateTimeOffset ExpiresAt);

    private sealed class TokenState(string value, DateTimeOffset obtainedAt, Lifetime? lifetime)
    {
        private volatile Lifetime? _lifetime = lifetime;
        private volatile bool _settled = lifetime is not null;

        public Lazy<Task<bool>>? Probe;

        public string Value { get; } = value;

        public DateTimeOffset ObtainedAt { get; } = obtainedAt;

        public Lifetime? Lifetime
        {
            get => _lifetime;
            set => _lifetime = value;
        }

        /// <summary>Lifetime known, or the one probe for this token already ran.</summary>
        public bool Settled
        {
            get => _settled;
            set => _settled = value;
        }
    }
}
