using Postway.Core.ViewModels.ShipmentProviders;
using Postway.Data.Services;

namespace Postway.Data.Instances;

public interface IShipmentProviderPost : IDisposable
{
    Task<List<AllResponse>> All();
}

internal class ShipmentProviderPost : IShipmentProviderPost
{
    private readonly string _accessToken;
    private readonly string _baseUrl;
    private readonly IHttpClientService _httpClientService;

    public ShipmentProviderPost(string accessToken, string baseUrl, IHttpClientService httpClientService)
    {
        _accessToken = accessToken;
        _baseUrl = baseUrl;
        _httpClientService = httpClientService;
    }

    public async Task<List<AllResponse>> All()
    {
        var url = $"{_baseUrl}/shipment-provider/all";
        var headers = new Dictionary<string, string>
        {
            { "Authorization", $"Bearer {_accessToken}" },
            { "Content-Type", "application/json" },
        };

        var response = await _httpClientService.GetAsync<List<AllResponse>>(url, headers);
        if (response == null)
        {
            throw new Exception("Failed to retrieve shipment providers.");
        }

        return response;
    }

    public void Dispose()
    {
    }
}