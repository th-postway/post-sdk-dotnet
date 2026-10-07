// Live, read-only checks against a real Merchant API.
//
//   POSTWAY_MERCHANT_BASE_URL=https://sandbox-post.postway.co.th/merchant \
//   POSTWAY_MERCHANT_ACCESS_TOKEN=... dotnet test tests/IntegrationTest
//
// Skipped unless both variables are set. Never creates or cancels anything.

using Postway.Models;

namespace Postway.IntegrationTest;

public sealed class MerchantIntegrationTests : IDisposable
{
    private static readonly string? EnvBaseUrl = Environment.GetEnvironmentVariable("POSTWAY_MERCHANT_BASE_URL");
    private static readonly string? EnvAccessToken = Environment.GetEnvironmentVariable("POSTWAY_MERCHANT_ACCESS_TOKEN");

    private readonly PostwayMerchantClient? _client;

    public MerchantIntegrationTests()
    {
        if (Configured)
        {
            _client = new PostwayMerchantClient(new PostwayMerchantClientOptions { BaseUrl = EnvBaseUrl, AccessToken = EnvAccessToken });
        }
    }

    private static bool Configured => !string.IsNullOrEmpty(EnvBaseUrl) && !string.IsNullOrEmpty(EnvAccessToken);

    private PostwayMerchantClient Client
    {
        get
        {
            Assert.SkipUnless(Configured, "set POSTWAY_MERCHANT_BASE_URL and POSTWAY_MERCHANT_ACCESS_TOKEN to run live tests");
            return _client!;
        }
    }

    public void Dispose() => _client?.Dispose();

    [Fact]
    public async Task HealthPing() => Assert.Equal("pong", await Client.Health.PingAsync(cancellationToken: TestContext.Current.CancellationToken));

    [Fact]
    public async Task AuthAccountInfo()
    {
        var info = await Client.Auth.AccountInfoAsync(cancellationToken: TestContext.Current.CancellationToken);
        Assert.False(string.IsNullOrEmpty(info.Store.Code));
        Assert.True(info.Session.Expired > DateTimeOffset.UtcNow);
    }

    [Fact]
    public async Task RejectsAnInvalidTokenWith403()
    {
        _ = Client;
        using var anonymous = new PostwayMerchantClient(new PostwayMerchantClientOptions { BaseUrl = EnvBaseUrl, AccessToken = "invalid-token" });
        var error = await Assert.ThrowsAsync<PostwayApiException>(
            () => anonymous.Auth.AccountInfoAsync(cancellationToken: TestContext.Current.CancellationToken));
        Assert.Equal(403, error.Status);
    }

    [Fact]
    public async Task ShipmentProvidersAll()
    {
        var providers = await Client.ShipmentProviders.AllAsync(cancellationToken: TestContext.Current.CancellationToken);
        Assert.NotNull(providers);
    }

    [Fact]
    public async Task ThailandFilter()
    {
        var page = await Client.Thailand.FilterAsync(
            new MerchantThailandFilterRequest { Page = 1, Limit = 5 },
            cancellationToken: TestContext.Current.CancellationToken);
        Assert.Equal(5, page.Limit);
        Assert.True(page.Data.Count <= 5);
    }

    [Fact]
    public async Task OrderShipmentsFilter()
    {
        var page = await Client.OrderShipments.FilterAsync(
            new MerchantOrderShipmentFilterRequest { Page = 1, Limit = 5 },
            cancellationToken: TestContext.Current.CancellationToken);
        Assert.True(page.Count >= 0);
    }

    [Fact]
    public async Task OrderShipmentsGetByTrackingNoReturnsNullForAnUnknownNumber() =>
        Assert.Null(await Client.OrderShipments.GetByTrackingNoAsync(
            $"SDK-NOPE-{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}",
            cancellationToken: TestContext.Current.CancellationToken));
}
