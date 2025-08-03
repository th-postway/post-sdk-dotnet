using System.Text;
using Postway.Core.Extensions;
using Postway.Core.ViewModels.Labels;
using Postway.Data.Services;

namespace Postway.Data.Instances;

public interface ILabelPost : IDisposable
{
    Task<OrderShipmentResponse> OrderShipment(OrderShipmentRequest request);
}

internal class LabelPost : ILabelPost
{
    private readonly string _accessToken;
    private readonly string _baseUrl;
    private readonly IHttpClientService _httpClientService;

    public LabelPost(string accessToken, string baseUrl, IHttpClientService httpClientService)
    {
        _accessToken = accessToken;
        _baseUrl = baseUrl;
        _httpClientService = httpClientService;
    }

    public async Task<OrderShipmentResponse> OrderShipment(OrderShipmentRequest request)
    {
        var url = $"{_baseUrl}/label/order/shipments";

        var headers = new Dictionary<string, string>
        {
            { "Authorization", $"Bearer {_accessToken}" },
            { "Content-Type", "application/json" }
        };

        // Serialize the request object to JSON
        using var content = new StringContent(request.ToJson(), Encoding.UTF8, "application/json");

        // Make the POST request
        var response = await _httpClientService.PostAsync<OrderShipmentResponse>(url, content, headers);
        if (response == null)
        {
            throw new Exception("Failed to create order shipment label.");
        }

        return response;
    }

    public void Dispose()
    {
    }
}