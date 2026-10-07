using System.Text.Json;
using System.Text.Json.Nodes;
using Postway.Models;

namespace Postway.Resources;

/// <summary><c>thailand/*</c> — Thai postal areas with courier coverage and surcharge flags.</summary>
public sealed class ThailandResource
{
    private readonly HttpPipeline _http;

    internal ThailandResource(HttpPipeline http) => _http = http;

    /// <summary>Search postal areas. <c>POST thailand/filter</c>.</summary>
    public async Task<FilterResponse<MerchantThailand>> FilterAsync(
        MerchantThailandFilterRequest request,
        RequestOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        // older servers return 500 when the array is missing, so always send it
        var body = JsonSerializer.SerializeToNode(request, HttpPipeline.JsonOptions)!.AsObject();
        body["shipment_provider_names"] ??= new JsonArray();

        return await _http.RequestAsync<FilterResponse<MerchantThailand>>(
            new HttpCall(HttpMethod.Post, ["thailand", "filter"]) { Body = body, Auth = true, Options = options },
            cancellationToken).ConfigureAwait(false) ?? new FilterResponse<MerchantThailand>();
    }
}
