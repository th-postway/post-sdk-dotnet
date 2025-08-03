using Postway.Core.ViewModels.Auths;
using Postway.Data.Services;

namespace Postway.Data.Instances;

public interface IAuthPost : IDisposable
{
    Task<AccountInfoResponse> AccountInfo();
}

internal class AuthPost : IAuthPost
{
    private readonly string _accessToken;
    private readonly string _baseUrl;
    private readonly IHttpClientService _httpClientService;

    public AuthPost(string accessToken, string baseUrl, IHttpClientService httpClientService)
    {
        _baseUrl = baseUrl;
        _httpClientService = httpClientService;
        _accessToken = accessToken;
    }

    public async Task<AccountInfoResponse> AccountInfo()
    {
        var url = $"{_baseUrl}/auth/account/info";
        var headers = new Dictionary<string, string>
        {
            { "Authorization", $"Bearer {_accessToken}" },
            { "Content-Type", "application/json" }
        };

        var response = await _httpClientService.GetAsync<AccountInfoResponse>(url, headers);
        if (response == null)
        {
            throw new Exception("Failed to retrieve account information.");
        }

        return response;
    }

    public void Dispose()
    {
    }
}