using Postway.Models;

namespace Postway.Resources;

/// <summary><c>shipment-provider/*</c> — couriers available to the store.</summary>
public sealed class ShipmentProvidersResource
{
    private readonly HttpPipeline _http;

    internal ShipmentProvidersResource(HttpPipeline http) => _http = http;

    /// <summary>Active couriers after the store's white/blacklist, sorted by name. <c>GET shipment-provider/all</c>.</summary>
    public async Task<IReadOnlyList<MerchantShipmentProviderData>> AllAsync(
        RequestOptions? options = null,
        CancellationToken cancellationToken = default) =>
        await _http.RequestAsync<List<MerchantShipmentProviderData>>(
            new HttpCall(HttpMethod.Get, ["shipment-provider", "all"]) { Auth = true, Options = options },
            cancellationToken).ConfigureAwait(false) ?? [];
}
