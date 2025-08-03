using System.Text;
using Postway.Core.Extensions;
using Postway.Core.ViewModels.OrderShipments;
using Postway.Data.Services;

namespace Postway.Data.Instances;

public interface IOrderShipmentPost : IDisposable
{
    Task<GetByTrackingNoResponse> OrderShipment_GetByTrackingNo(string trackingNo);
    Task<GetByRefResponse> OrderShipment_GetByRef(string refNo);
    Task<FilterResponse> Filter(FilterRequest request);
    Task<CreateResponse> Create(CreateRequest request);
    Task<CalculatePriceResponse> CalculatePrice(CalculatePriceRequest request);
    Task<CancelResponse> Cancel(CancelRequest request);
}

internal class OrderShipmentPost : IOrderShipmentPost
{
    private readonly string _accessToken;
    private readonly string _baseUrl;
    private readonly IHttpClientService _httpClientService;

    public OrderShipmentPost(string accessToken, string baseUrl, IHttpClientService httpClientService)
    {
        _accessToken = accessToken;
        _baseUrl = baseUrl;
        _httpClientService = httpClientService;
    }

    public async Task<GetByTrackingNoResponse> OrderShipment_GetByTrackingNo(string trackingNo)
    {
        var url = $"{_baseUrl}/order-shipment/get-by-tracking-no/{trackingNo}";
        var headers = new Dictionary<string, string>
        {
            { "Authorization", $"Bearer {_accessToken}" },
            { "Content-Type", "application/json" },
        };

        var response = await _httpClientService.GetAsync<GetByTrackingNoResponse>(url, headers);
        if (response == null)
        {
            throw new Exception($"Failed to retrieve order shipment for tracking number: {trackingNo}");
        }

        return response;
    }

    public async Task<GetByRefResponse> OrderShipment_GetByRef(string refNo)
    {
        var url = $"{_baseUrl}/order-shipment/get-by-ref/{refNo}";
        var headers = new Dictionary<string, string>
        {
            { "Authorization", $"Bearer {_accessToken}" },
            { "Content-Type", "application/json" },
        };

        var response = await _httpClientService.GetAsync<GetByRefResponse>(url, headers);
        if (response == null)
        {
            throw new Exception($"Failed to retrieve order shipment for reference number: {refNo}");
        }

        return response;
    }

    public async Task<FilterResponse> Filter(FilterRequest request)
    {
        var url = $"{_baseUrl}/order-shipment/filter";
        var headers = new Dictionary<string, string>
        {
            { "Authorization", $"Bearer {_accessToken}" },
            { "Content-Type", "application/json" }
        };

        var content = new StringContent(request.ToJson(), Encoding.UTF8, "application/json");
        var response = await _httpClientService.PostAsync<FilterResponse>(url, content, headers);
        if (response == null)
        {
            throw new Exception("Failed to filter order shipments.");
        }

        return response;
    }

    public async Task<CreateResponse> Create(CreateRequest request)
    {
        var url = $"{_baseUrl}/order-shipment/create";
        var headers = new Dictionary<string, string>
        {
            { "Authorization", $"Bearer {_accessToken}" },
            { "Content-Type", "application/json" }
        };

        using var content = new StringContent(request.ToJson(), Encoding.UTF8, "application/json");
        var response = await _httpClientService.PostAsync<CreateResponse>(url, content, headers);
        if (response == null)
        {
            throw new Exception("Failed to create order shipment.");
        }

        return response;
    }

    public async Task<CalculatePriceResponse> CalculatePrice(CalculatePriceRequest request)
    {
        var url = $"{_baseUrl}/order-shipment/calculate-price";
        var headers = new Dictionary<string, string>
        {
            { "Authorization    ", $"Bearer {_accessToken}" },
            { "Content-Type", "application/json" }
        };

        using var content = new StringContent(request.ToJson(), Encoding.UTF8, "application/json");
        var response = await _httpClientService.PostAsync<CalculatePriceResponse>(url, content, headers);
        if (response == null)
        {
            throw new Exception("Failed to calculate order shipment price.");
        }

        return response;
    }

    public async Task<CancelResponse> Cancel(CancelRequest request)
    {
        var url = $"{_baseUrl}/order-shipment/cancel";
        var headers = new Dictionary<string, string>
        {
            { "Authorization", $"Bearer {_accessToken}" },
            { "Content-Type", "application/json" }
        };

        using var content = new StringContent(request.ToJson(), Encoding.UTF8, "application/json");
        var response = await _httpClientService.PostAsync<CancelResponse>(url, content, headers);
        if (response == null)
        {
            throw new Exception("Failed to cancel order shipment.");
        }

        return response;
    }

    public void Dispose()
    {
    }
}