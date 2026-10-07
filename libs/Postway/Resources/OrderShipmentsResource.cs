using Postway.Models;

namespace Postway.Resources;

/// <summary><c>order-shipment/*</c> — the store's parcels. Every call is scoped to the token's store.</summary>
public sealed class OrderShipmentsResource
{
    private readonly HttpPipeline _http;

    internal OrderShipmentsResource(HttpPipeline http) => _http = http;

    /// <summary>Find a parcel by courier tracking number; <c>null</c> when none. <c>GET order-shipment/get-by-tracking-no/:tracking_no</c>.</summary>
    public Task<MerchantOrderShipmentData?> GetByTrackingNoAsync(
        string trackingNo,
        RequestOptions? options = null,
        CancellationToken cancellationToken = default) =>
        _http.RequestAsync<MerchantOrderShipmentData>(
            new HttpCall(
                HttpMethod.Get,
                ["order-shipment", "get-by-tracking-no", PathSegment.Param("tracking_no", trackingNo)])
            {
                Auth = true,
                Options = options,
            },
            cancellationToken);

    /// <summary>Find a parcel whose <c>ref1</c>, <c>ref2</c> or <c>ref3</c> equals <paramref name="reference"/>; <c>null</c> when none. <c>GET order-shipment/get-by-ref/:ref</c>.</summary>
    public Task<MerchantOrderShipmentData?> GetByRefAsync(
        string reference,
        RequestOptions? options = null,
        CancellationToken cancellationToken = default) =>
        _http.RequestAsync<MerchantOrderShipmentData>(
            new HttpCall(HttpMethod.Get, ["order-shipment", "get-by-ref", PathSegment.Param("ref", reference)])
            {
                Auth = true,
                Options = options,
            },
            cancellationToken);

    /// <summary>Page through the store's parcels, newest first. <c>POST order-shipment/filter</c>.</summary>
    public async Task<FilterResponse<MerchantOrderShipmentData>> FilterAsync(
        MerchantOrderShipmentFilterRequest request,
        RequestOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        return await _http.RequestAsync<FilterResponse<MerchantOrderShipmentData>>(
            new HttpCall(HttpMethod.Post, ["order-shipment", "filter"]) { Body = request, Auth = true, Options = options },
            cancellationToken).ConfigureAwait(false) ?? new FilterResponse<MerchantOrderShipmentData>();
    }

    /// <summary>Create one parcel; see <see cref="CreateAsync(IEnumerable{MerchantOrderShipmentCreateRequest}, RequestOptions?, CancellationToken)"/>.</summary>
    public Task<IReadOnlyList<MerchantOrderShipmentData>> CreateAsync(
        MerchantOrderShipmentCreateRequest request,
        RequestOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        return CreateAsync([request], options, cancellationToken);
    }

    /// <summary>
    /// Create one or more parcels, book them with the courier and issue the receipt. <c>POST order-shipment/create</c>.
    /// </summary>
    /// <remarks>
    /// Not idempotent and never retried by the SDK. The batch stops at the first failure: parcels created before it
    /// remain, so on <see cref="PostwayBusinessException"/> look them up by <c>my_tracking_no</c> before resubmitting.
    /// </remarks>
    /// <returns>The created parcels, including their courier <c>tracking_no</c>.</returns>
    /// <exception cref="PostwayBusinessException">Verification, creation or receipt issue failed.</exception>
    public async Task<IReadOnlyList<MerchantOrderShipmentData>> CreateAsync(
        IEnumerable<MerchantOrderShipmentCreateRequest> requests,
        RequestOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(requests);
        List<MerchantOrderShipmentCreateRequest> body = [.. requests];
        return await _http.RequestEnvelopeAsync<List<MerchantOrderShipmentData>>(
            new HttpCall(HttpMethod.Post, ["order-shipment", "create"]) { Body = body, Auth = true, Options = options },
            cancellationToken).ConfigureAwait(false) ?? [];
    }

    /// <summary>Quote the price of one parcel without creating it. <c>POST order-shipment/calculate-price</c>.</summary>
    public async Task<MerchantOrderShipmentCalculatePriceResponse> CalculatePriceAsync(
        MerchantOrderShipmentCalculatePriceRequest request,
        RequestOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        return await _http.RequestAsync<MerchantOrderShipmentCalculatePriceResponse>(
            new HttpCall(HttpMethod.Post, ["order-shipment", "calculate-price"]) { Body = request, Auth = true, Options = options },
            cancellationToken).ConfigureAwait(false) ?? new MerchantOrderShipmentCalculatePriceResponse();
    }

    /// <summary>
    /// Cancel a parcel by courier tracking number (not <c>my_tracking_no</c> or refs). <c>POST order-shipment/cancel</c>.
    /// </summary>
    /// <exception cref="PostwayApiException">400 when the parcel is not found.</exception>
    /// <exception cref="PostwayBusinessException">The cancellation was refused.</exception>
    public async Task CancelAsync(
        string trackingNo,
        RequestOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(trackingNo);
        await _http.RequestEnvelopeAsync<object>(
            new HttpCall(HttpMethod.Post, ["order-shipment", "cancel"])
            {
                Body = new Dictionary<string, string> { ["tracking_no"] = trackingNo },
                Auth = true,
                Options = options,
            },
            cancellationToken).ConfigureAwait(false);
    }
}
