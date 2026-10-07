using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Postway.UnitTest.Support;
using static Postway.UnitTest.Support.Stub;

namespace Postway.UnitTest.Core;

public class AccessTokenTests
{
    private static readonly DateTimeOffset T0 = new(2027, 1, 1, 0, 0, 0, TimeSpan.Zero);
    private const string AccountInfo = $"{BaseUrl}/auth/account/info";
    private const string Providers = $"{BaseUrl}/shipment-provider/all";

    /// <summary>Unsigned JWT with the given <c>iat</c> / <c>exp</c>. Only the payload matters to the SDK.</summary>
    private static string Jwt(DateTimeOffset issuedAt, DateTimeOffset expiresAt, string subject = "shop")
    {
        static string Part(object value) =>
            Convert.ToBase64String(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(value))).TrimEnd('=').Replace('+', '-').Replace('/', '_');
        return $"{Part(new { alg = "none" })}.{Part(new { sub = subject, iat = issuedAt.ToUnixTimeSeconds(), exp = expiresAt.ToUnixTimeSeconds() })}.sig";
    }

    private static string Bearer(string token) => $"Bearer {token}";

    private static HttpResponseMessage Session(DateTimeOffset expired) =>
        Json(new JsonObject { ["store"] = new JsonObject(), ["user"] = new JsonObject(), ["session"] = new JsonObject { ["expired"] = expired.ToString("O") } });

    private static HttpResponseMessage Ok() => Envelope(200, 200, true, "ok", null);

    /// <summary>Provider that hands out <paramref name="tokens"/> in order and records why it was asked.</summary>
    private sealed class Provider(params AccessToken[] tokens)
    {
        private readonly Queue<AccessToken> _tokens = new(tokens);

        public List<AccessTokenRefreshReason> Reasons { get; } = [];

        public ValueTask<AccessToken> GetAsync(AccessTokenRefreshReason reason, CancellationToken cancellationToken)
        {
            lock (Reasons)
            {
                Reasons.Add(reason);
                return ValueTask.FromResult(_tokens.Count > 0 ? _tokens.Dequeue() : throw new InvalidOperationException("no more tokens"));
            }
        }
    }

    private static (PostwayMerchantClient Client, StubHandler Handler, FakeTimeProvider Clock) Client(
        Provider provider,
        string? accessToken,
        params HttpResponseMessage[] responses)
    {
        var clock = new FakeTimeProvider(T0);
        var (client, handler) = Setup(responses, o =>
        {
            o.AccessToken = accessToken;
            o.AccessTokenProvider = provider.GetAsync;
            o.TimeProvider = clock;
        });
        return (client, handler, clock);
    }

    [Fact]
    public async Task AsksTheProviderOnceForTheFirstTokenAndReusesItWithoutProbingAJwt()
    {
        var token = Jwt(T0, T0.AddSeconds(100));
        var provider = new Provider(new AccessToken(token));
        var (client, handler, _) = Client(provider, null, Json("[]"), Json("[]"));
        await client.ShipmentProviders.AllAsync();
        await client.ShipmentProviders.AllAsync();
        Assert.Equal([AccessTokenRefreshReason.Initial], provider.Reasons);
        Assert.All(handler.Calls, call => Assert.Equal(Providers, call.Url));
        Assert.All(handler.Calls, call => Assert.Equal(Bearer(token), call.Headers["Authorization"]));
    }

    [Fact]
    public async Task RefreshesOnceSeventyFivePercentOfTheJwtLifetimeHasElapsed()
    {
        var first = Jwt(T0.AddSeconds(-10), T0.AddSeconds(90));
        var second = Jwt(T0.AddSeconds(65), T0.AddSeconds(165));
        var provider = new Provider(new AccessToken(first), new AccessToken(second));
        var (client, handler, clock) = Client(provider, null, Json("[]"), Json("[]"), Json("[]"));

        await client.ShipmentProviders.AllAsync();
        clock.Now = T0.AddSeconds(65).AddMilliseconds(-1);
        await client.ShipmentProviders.AllAsync();
        clock.Now = T0.AddSeconds(65);
        await client.ShipmentProviders.AllAsync();

        Assert.Equal([AccessTokenRefreshReason.Initial, AccessTokenRefreshReason.Expiring], provider.Reasons);
        Assert.Equal([Bearer(first), Bearer(first), Bearer(second)], handler.Calls.Select(c => c.Headers["Authorization"]));
    }

    [Fact]
    public async Task UsesTheExpiresAtTheProviderReturnsForAnOpaqueTokenWithoutProbing()
    {
        var provider = new Provider(new AccessToken("opaque_1", T0.AddSeconds(100)), new AccessToken("opaque_2", T0.AddSeconds(200)));
        var (client, handler, clock) = Client(provider, null, Json("[]"), Json("[]"));
        await client.ShipmentProviders.AllAsync();
        clock.Now = T0.AddSeconds(75);
        await client.ShipmentProviders.AllAsync();
        Assert.Equal([AccessTokenRefreshReason.Initial, AccessTokenRefreshReason.Expiring], provider.Reasons);
        Assert.Equal([Bearer("opaque_1"), Bearer("opaque_2")], handler.Calls.Select(c => c.Headers["Authorization"]));
    }

    [Fact]
    public async Task ProbesAccountInfoOnceForAnOpaqueTokenAndRefreshesAtSeventyFivePercentOfSessionExpired()
    {
        var next = Jwt(T0.AddSeconds(75), T0.AddSeconds(175));
        var provider = new Provider(new AccessToken(next));
        var (client, handler, clock) = Client(provider, "opaque_static", Session(T0.AddSeconds(100)), Json("[]"), Json("[]"), Json("[]"));

        await client.ShipmentProviders.AllAsync();
        await client.ShipmentProviders.AllAsync();
        Assert.Equal([AccountInfo, Providers, Providers], handler.Calls.Select(c => c.Url));
        Assert.Equal("POST", handler.Calls[0].Method);
        Assert.Equal(Bearer("opaque_static"), handler.Calls[0].Headers["Authorization"]);
        Assert.Empty(provider.Reasons);

        clock.Now = T0.AddSeconds(75);
        await client.ShipmentProviders.AllAsync();
        Assert.Equal([AccessTokenRefreshReason.Expiring], provider.Reasons);
        Assert.Equal(Bearer(next), handler.Calls[3].Headers["Authorization"]);
    }

    [Fact]
    public async Task LearnsTheLifetimeFromTheCallersOwnAccountInfoWithoutAnExtraProbe()
    {
        var next = Jwt(T0.AddSeconds(75), T0.AddSeconds(175));
        var provider = new Provider(new AccessToken(next));
        var (client, handler, clock) = Client(provider, "opaque_static", Session(T0.AddSeconds(100)), Json("[]"));
        await client.Auth.AccountInfoAsync();
        clock.Now = T0.AddSeconds(75);
        await client.ShipmentProviders.AllAsync();
        Assert.Equal(2, handler.RequestCount);
        Assert.Equal([AccessTokenRefreshReason.Expiring], provider.Reasons);
        Assert.Equal(Bearer(next), handler.Calls[1].Headers["Authorization"]);
    }

    [Fact]
    public async Task RefreshesStraightAwayWhenTheProbeGetsA403()
    {
        var next = Jwt(T0, T0.AddSeconds(100));
        var provider = new Provider(new AccessToken(next));
        var (client, handler, _) = Client(provider, "opaque_stale", ApiError(403, "Forbidden resource"), Json("[]"));
        await client.ShipmentProviders.AllAsync();
        Assert.Equal([AccessTokenRefreshReason.Forbidden], provider.Reasons);
        Assert.Equal([AccountInfo, Providers], handler.Calls.Select(c => c.Url));
        Assert.Equal([Bearer("opaque_stale"), Bearer(next)], handler.Calls.Select(c => c.Headers["Authorization"]));
    }

    [Fact]
    public async Task CarriesOnWithTheCallWhenTheProbeFailsForAnotherReason()
    {
        var provider = new Provider();
        var (client, handler, _) = Client(provider, "opaque_static", ApiError(500, "Internal server error"), Json("[]"), Json("[]"));
        await client.ShipmentProviders.AllAsync();
        await client.ShipmentProviders.AllAsync();
        Assert.Empty(provider.Reasons);
        Assert.Equal([AccountInfo, Providers, Providers], handler.Calls.Select(c => c.Url));
    }

    [Fact]
    public async Task On403RefreshesAndReplaysTheCallOnceWithTheNewTokenAndTheSameBody()
    {
        var first = Jwt(T0, T0.AddSeconds(100), "first");
        var second = Jwt(T0, T0.AddSeconds(100), "second");
        var provider = new Provider(new AccessToken(first), new AccessToken(second));
        var (client, handler, _) = Client(provider, null, ApiError(403, "Forbidden resource"), Ok());

        await client.OrderShipments.CancelAsync("PW1");

        Assert.Equal([AccessTokenRefreshReason.Initial, AccessTokenRefreshReason.Forbidden], provider.Reasons);
        Assert.Equal(2, handler.RequestCount);
        Assert.Equal([Bearer(first), Bearer(second)], handler.Calls.Select(c => c.Headers["Authorization"]));
        Assert.Equal(handler.Calls[0].Method, handler.Calls[1].Method);
        Assert.Equal(handler.Calls[0].Url, handler.Calls[1].Url);
        Assert.Equal(handler.Calls[0].RawBody, handler.Calls[1].RawBody);
        JsonEqual("""{ "tracking_no": "PW1" }""", handler.Calls[1].Body);
    }

    [Fact]
    public async Task On403RefreshThen403ThrowsWithoutAThirdRequest()
    {
        var provider = new Provider(new AccessToken(Jwt(T0, T0.AddSeconds(100), "a")), new AccessToken(Jwt(T0, T0.AddSeconds(100), "b")));
        var (client, handler, _) = Client(provider, null, ApiError(403, "Forbidden resource"), ApiError(403, "Forbidden resource"));
        var error = await Assert.ThrowsAsync<PostwayApiException>(() => client.ShipmentProviders.AllAsync());
        Assert.Equal(403, error.Status);
        Assert.Equal(2, handler.RequestCount);
        Assert.Equal([AccessTokenRefreshReason.Initial, AccessTokenRefreshReason.Forbidden], provider.Reasons);
    }

    [Fact]
    public async Task DoesNotReplayAfterA403WhenTheProbeAlreadyUsedTheRefresh()
    {
        var provider = new Provider(new AccessToken("opaque_new"));
        var (client, handler, _) = Client(provider, "opaque_stale", ApiError(403, "Forbidden resource"), ApiError(403, "Forbidden resource"));
        await Assert.ThrowsAsync<PostwayApiException>(() => client.ShipmentProviders.AllAsync());
        Assert.Equal([AccessTokenRefreshReason.Forbidden], provider.Reasons);
        Assert.Equal(2, handler.RequestCount);
    }

    [Fact]
    public async Task WithoutAProviderNeitherProbesRefreshesNorReplays()
    {
        var (client, handler) = Setup(ApiError(403, "Forbidden resource"));
        await Assert.ThrowsAsync<PostwayApiException>(() => client.ShipmentProviders.AllAsync());
        Assert.Equal(1, handler.RequestCount);
    }

    [Fact]
    public async Task NeverProbesRefreshesAuthenticatesOrReplaysPublicRoutes()
    {
        var provider = new Provider();
        var (client, handler, _) = Client(provider, null, Text("pong"), ApiError(403, "Forbidden resource"));
        await client.Health.PingAsync();
        await Assert.ThrowsAsync<PostwayApiException>(() => client.Receipts.GetPublicAsync("rcpt"));
        Assert.Empty(provider.Reasons);
        Assert.Equal(2, handler.RequestCount);
        Assert.All(handler.Calls, call => Assert.False(call.Headers.ContainsKey("Authorization")));
    }

    [Fact]
    public async Task SharesOneProviderCallAndOneProbeBetweenConcurrentCalls()
    {
        var release = new TaskCompletionSource<AccessToken>(TaskCreationOptions.RunContinuationsAsynchronously);
        var calls = 0;
        var clock = new FakeTimeProvider(T0);
        var (client, handler) = Setup([Session(T0.AddSeconds(100)), Json("[]"), Json("[]"), Json("[]")], o =>
        {
            o.AccessToken = null;
            o.TimeProvider = clock;
            o.AccessTokenProvider = (_, _) =>
            {
                Interlocked.Increment(ref calls);
                return new ValueTask<AccessToken>(release.Task);
            };
        });

        var pending = Task.WhenAll(client.ShipmentProviders.AllAsync(), client.ShipmentProviders.AllAsync(), client.ShipmentProviders.AllAsync());
        release.SetResult(new AccessToken("opaque_1"));
        await pending;

        Assert.Equal(1, calls);
        Assert.Single(handler.Calls, c => c.Url == AccountInfo);
        Assert.Equal(4, handler.RequestCount);
    }

    [Fact]
    public async Task RejectsAnUnsafeTokenFromTheProviderWithoutEchoingItBeforeAnyRequest()
    {
        var provider = new Provider(new AccessToken("secret\r\nX-Injected: 1"));
        var (client, handler, _) = Client(provider, null);
        var error = await Assert.ThrowsAsync<PostwayConfigException>(() => client.ShipmentProviders.AllAsync());
        Assert.DoesNotContain("secret", error.Message, StringComparison.Ordinal);
        Assert.Equal(0, handler.RequestCount);
    }

    [Fact]
    public async Task PropagatesAProviderExceptionUnchangedAndSendsNothing()
    {
        var failure = new InvalidOperationException("login service down");
        var (client, handler) = Setup([], o =>
        {
            o.AccessToken = null;
            o.AccessTokenProvider = (_, _) => throw failure;
        });
        var error = await Assert.ThrowsAsync<InvalidOperationException>(() => client.ShipmentProviders.AllAsync());
        Assert.Same(failure, error);
        Assert.Equal(0, handler.RequestCount);
    }

    [Fact]
    public void AccessTokenToStringHidesTheToken() =>
        Assert.DoesNotContain("secret", new AccessToken("secret", T0).ToString(), StringComparison.Ordinal);
}
