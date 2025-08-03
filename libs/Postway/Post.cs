using Postway.Data.Instances;
using Postway.Data.Services;

namespace Postway;

public interface IPost : IDisposable
{
    IAuthPost Auth { get; }
    ILabelPost Label { get; }
    IOrderShipmentPost OrderShipment { get; }
    IShipmentProviderPost ShipmentProvider { get; }
    IThailandPost Thailand { get; }
}

public class Post : IPost
{
    private readonly string _accessToken;
    private readonly string _baseUrl;
    private readonly IHttpClientService _httpClientService;

    public IAuthPost Auth { get; }
    public ILabelPost Label { get; }
    public IOrderShipmentPost OrderShipment { get; }
    public IShipmentProviderPost ShipmentProvider { get; }
    public IThailandPost Thailand { get; }

    public Post(string accessToken)
    {
        _accessToken = accessToken;
        _baseUrl = "https://post.postway.co.th/merchant";
        _httpClientService = new HttpClientService();

        Auth = new AuthPost(_accessToken, _baseUrl, _httpClientService);
        Label = new LabelPost(_accessToken, _baseUrl, _httpClientService);
        OrderShipment = new OrderShipmentPost(_accessToken, _baseUrl, _httpClientService);
        ShipmentProvider = new ShipmentProviderPost(_accessToken, _baseUrl, _httpClientService);
        Thailand = new ThailandPost(_accessToken, _baseUrl, _httpClientService);
    }

    public void Dispose()
    {
        Auth.Dispose();
        Label.Dispose();
        OrderShipment.Dispose();
        ShipmentProvider.Dispose();
        Thailand.Dispose();
        _httpClientService.Dispose();
    }
}