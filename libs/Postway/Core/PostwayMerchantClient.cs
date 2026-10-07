using Postway.Resources;

namespace Postway;

/// <summary>Client for the Postway Merchant API.</summary>
/// <remarks>
/// The constructor validates every option and throws <see cref="PostwayConfigException"/> without echoing the
/// offending value, so construction failures are safe to log. The client is thread-safe; create one and reuse it.
/// <code>
/// using var postway = new PostwayMerchantClient(new PostwayMerchantClientOptions { AccessToken = token });
/// var account = await postway.Auth.AccountInfoAsync();
/// </code>
/// </remarks>
public sealed class PostwayMerchantClient : IDisposable
{
    private readonly HttpPipeline _http;

    /// <summary>Creates a production client with <paramref name="accessToken"/>.</summary>
    public PostwayMerchantClient(string accessToken)
        : this(new PostwayMerchantClientOptions { AccessToken = accessToken }) { }

    /// <summary>Creates a client from <paramref name="options"/> (all defaults when <c>null</c>).</summary>
    /// <exception cref="PostwayConfigException">An option is invalid or unsafe.</exception>
    public PostwayMerchantClient(PostwayMerchantClientOptions? options = null)
    {
        options ??= new PostwayMerchantClientOptions();
        var baseUrl = options.BaseUrl ?? MerchantBaseUrls.For(options.Environment);
        _http = new HttpPipeline(
            baseUrl,
            string.IsNullOrEmpty(options.AccessToken) ? null : options.AccessToken,
            options.TokenType,
            options.Timeout,
            options.UserAgent ?? DefaultUserAgent(),
            options.HttpClient,
            options.AccessTokenProvider,
            options.TimeProvider ?? throw new PostwayConfigException("TimeProvider must not be null"));

        Auth = new AuthResource(_http);
        OrderShipments = new OrderShipmentsResource(_http);
        ShipmentProviders = new ShipmentProvidersResource(_http);
        Thailand = new ThailandResource(_http);
        Labels = new LabelsResource(_http);
        Receipts = new ReceiptsResource(_http);
        Health = new HealthResource(_http);
    }

    /// <summary><c>auth/*</c> — session introspection.</summary>
    public AuthResource Auth { get; }

    /// <summary><c>order-shipment/*</c> — the store's parcels.</summary>
    public OrderShipmentsResource OrderShipments { get; }

    /// <summary><c>shipment-provider/*</c> — couriers available to the store.</summary>
    public ShipmentProvidersResource ShipmentProviders { get; }

    /// <summary><c>thailand/*</c> — Thai postal areas.</summary>
    public ThailandResource Thailand { get; }

    /// <summary><c>label/*</c> — printable labels and receipts.</summary>
    public LabelsResource Labels { get; }

    /// <summary><c>receipt/*</c> — public receipts (no token sent).</summary>
    public ReceiptsResource Receipts { get; }

    /// <summary><c>health/*</c> — liveness.</summary>
    public HealthResource Health { get; }

    /// <summary>Resolved base URL, without a trailing slash.</summary>
    public string BaseUrl => _http.BaseUrl;

    /// <summary>Disposes the SDK-owned <see cref="HttpClient"/>. An injected one is left alone.</summary>
    public void Dispose() => _http.Dispose();

    private static string DefaultUserAgent() => $"postway-sdk-dotnet/{PostwaySdk.Version} dotnet/{System.Environment.Version}";
}
