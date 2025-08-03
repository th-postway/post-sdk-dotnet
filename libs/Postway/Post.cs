using System.Text;
using System.Text.Json;
using Postway.Libraries;
using Postway.ViewModels.Auths;
using Postway.ViewModels.Labels;
using Postway.ViewModels.OrderShipments;

namespace Postway;

public interface IPost : IDisposable
{
    Task<AccountInfoResponse> Auth_AccountInfo();
    Task<OrderShipmentResponse> Label_OrderShipment(OrderShipmentRequest request);
    Task<GetByTrackingNoResponse> OrderShipment_GetByTrackingNo(string trackingNo);
    Task<GetByRefResponse> OrderShipment_GetByRef(string refNo);
}

public class Post : IPost
{
    private readonly string _accessToken;

    private readonly IHttpClientService _httpClientService;
    private readonly string _baseUrl = "https://post.postway.co.th/merchant";

    public Post(string accessToken)
    {
        _accessToken = accessToken;
        _httpClientService = new HttpClientService();
    }

    public async Task<AccountInfoResponse> Auth_AccountInfo()
    {
        var url = $"{_baseUrl}/auth/account/info";

        var headers = new Dictionary<string, string>
        {
            { "Accept", "*/*" },
            { "Authorization", $"Bearer {_accessToken}" },
            { "Content-Type", "application/json" }
        };
        
        // Create an empty content object for the request
        var content = new StringContent("", Encoding.UTF8, "application/json");

        // Make the POST request
        var response = await _httpClientService.PostAsync<AccountInfoResponse>(url, content, headers);
        if (response == null)
        {
            throw new Exception("Failed to retrieve account information.");
        }

        return response;
    }

    public async Task<OrderShipmentResponse> Label_OrderShipment(OrderShipmentRequest request)
    {
        var url = $"{_baseUrl}/label/order/shipments";

        var headers = new Dictionary<string, string>
        {
            { "Authorization", $"Bearer {_accessToken}" },
            { "Content-Type", "application/json" }
        };

        // Serialize the request object to JSON
        var content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");

        // Make the POST request
        var response = await _httpClientService.PostAsync<OrderShipmentResponse>(url, content, headers);
        if (response == null)
        {
            throw new Exception("Failed to create order shipment label.");
        }

        return response;
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

    public void Dispose()
    {
    }
}