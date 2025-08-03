using System.Text;
using Postway.Extensions;
using Postway.Libraries;
using Postway.ViewModels.Thailands;

namespace Postway;

public interface IThailandPost : IDisposable
{
    Task<FilterResponse> Filter(FilterRequest request);
}

public class ThailandPost : IThailandPost
{
    private readonly string _accessToken;
    private readonly string _baseUrl;
    private readonly IHttpClientService _httpClientService;

    public ThailandPost(string accessToken, string baseUrl, IHttpClientService httpClientService)
    {
        _accessToken = accessToken;
        _baseUrl = baseUrl;
        _httpClientService = httpClientService;
    }

    public async Task<FilterResponse> Filter(FilterRequest request)
    {
        var url = $"{_baseUrl}/thailand/filter";
        var headers = new Dictionary<string, string>
        {
            { "Authorization", $"Bearer {_accessToken}" },
            { "Content-Type", "application/json" },
        };

        var content = new StringContent(request.ToJson(), Encoding.UTF8, "application/json");
        var response = await _httpClientService.PostAsync<FilterResponse>(url, content, headers);
        if (response == null)
        {
            throw new Exception("Failed to retrieve Thailand filter response.");
        }

        return response;
    }

    public void Dispose()
    {
    }
}