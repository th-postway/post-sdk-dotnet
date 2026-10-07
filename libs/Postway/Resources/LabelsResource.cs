using Postway.Models;

namespace Postway.Resources;

/// <summary><c>label/*</c> — printable shipping labels and receipts, returned as base64 files.</summary>
public sealed class LabelsResource
{
    private readonly HttpPipeline _http;

    internal LabelsResource(HttpPipeline http) => _http = http;

    /// <summary>
    /// One file containing the labels of every matching parcel. <c>POST label/order/shipments</c>.
    /// Decode with <see cref="FileHttpResponse.DecodeContent"/>.
    /// </summary>
    /// <exception cref="PostwayApiException">400 when none of <c>tracking_nos</c> matches a parcel of the store.</exception>
    public async Task<FileHttpResponse> OrderShipmentsAsync(
        MerchantLabelOrderShipmentsRequest request,
        RequestOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        return await _http.RequestAsync<FileHttpResponse>(
            new HttpCall(HttpMethod.Post, ["label", "order", "shipments"]) { Body = request, Auth = true, Options = options },
            cancellationToken).ConfigureAwait(false) ?? new FileHttpResponse();
    }

    /// <summary>A printable receipt by receipt number. <c>GET label/receipt/:receipt_no?receipt_size=</c>.</summary>
    /// <param name="receiptNo">Receipt number.</param>
    /// <param name="receiptSize">A <see cref="ReceiptSize"/> value; omitted when <c>null</c>.</param>
    /// <param name="options">Per-call options.</param>
    /// <param name="cancellationToken">Cancels the call.</param>
    public async Task<FileHttpResponse> ReceiptAsync(
        string receiptNo,
        string? receiptSize = null,
        RequestOptions? options = null,
        CancellationToken cancellationToken = default) =>
        await _http.RequestAsync<FileHttpResponse>(
            new HttpCall(HttpMethod.Get, ["label", "receipt", PathSegment.Param("receipt_no", receiptNo)])
            {
                Query = [new("receipt_size", receiptSize)],
                Auth = true,
                Options = options,
            },
            cancellationToken).ConfigureAwait(false) ?? new FileHttpResponse();
}
