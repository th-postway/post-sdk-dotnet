using System.Text.Json;
using Postway.Core.Extensions;

namespace Postway.Data.Services;

public interface IHttpClientService : IDisposable
{
    void SetHeader(HttpClient httpClient, Dictionary<string, string> headers);
    Task<T?> GetAsync<T>(string url, Dictionary<string, string>? headers = null) where T : class;
    Task<T?> PostAsync<T>(string url, HttpContent content, Dictionary<string, string>? headers = null) where T : class;
    Task<T?> PutAsync<T>(string url, HttpContent content, Dictionary<string, string>? headers = null) where T : class;
    Task<T?> DeleteAsync<T>(string url) where T : class;
}

public class HttpClientService : IHttpClientService, IDisposable
{
    public void SetHeader(HttpClient httpClient, Dictionary<string, string> headers)
    {
        foreach (var header in headers)
        {
            if (httpClient.DefaultRequestHeaders.Contains(header.Key))
            {
                httpClient.DefaultRequestHeaders.Remove(header.Key);
            }
            httpClient.DefaultRequestHeaders.Add(header.Key, header.Value);
        }
    }

    public async Task<T?> GetAsync<T>(string url, Dictionary<string, string>? headers = null) where T : class
    {
        T? result = null;
        try
        {
            // set up HttpClient
            using var httpClient = new HttpClient();
            if (headers != null)
                SetHeader(httpClient, headers);

            // http request
            using var response = await httpClient.GetAsync(url);

            // ensure success status code
            response.EnsureSuccessStatusCode();

            // read and deserialize response
            var strJson = await response.Content.ReadAsStringAsync();
            result = strJson.FromJson<T>();
        }
        catch (HttpRequestException)
        {
            throw;
        }
        catch (JsonException)
        {
            throw;
        }
        return result;
    }

    public async Task<T?> PostAsync<T>(string url, HttpContent content, Dictionary<string, string>? headers = null) where T : class
    {
        T? result = null;
        try
        {
            using var httpClient = new HttpClient();
            if (headers != null)
                SetHeader(httpClient, headers);

            // Post request
            using var response = await httpClient.PostAsync(url, content);

            // Ensure the response indicates success
            response.EnsureSuccessStatusCode();

            var strJson = await response.Content.ReadAsStringAsync();
            result = strJson.FromJson<T>();
        }
        catch (HttpRequestException)
        {
            throw;
        }
        catch (JsonException)
        {
            throw;
        }
        return result;
    }

    public async Task<T?> PutAsync<T>(string url, HttpContent content, Dictionary<string, string>? headers = null) where T : class
    {
        T? result = null;
        try
        {
            using var httpClient = new HttpClient();
            if (headers != null)
                SetHeader(httpClient, headers);

            // Put request
            using var response = await httpClient.PutAsync(url, content);

            // Ensure the response indicates success
            response.EnsureSuccessStatusCode();

            // Read and deserialize response
            var strJson = await response.Content.ReadAsStringAsync();
            result = strJson.FromJson<T>();
        }
        catch (HttpRequestException)
        {
            throw;
        }
        catch (JsonException)
        {
            throw;
        }
        return result;
    }

    public async Task<T?> DeleteAsync<T>(string url) where T : class
    {
        T? result = null;
        try
        {
            using var httpClient = new HttpClient();
            // Delete request
            using var response = await httpClient.DeleteAsync(url);

            // Ensure the response indicates success
            response.EnsureSuccessStatusCode();

            // Read and deserialize response
            var strJson = await response.Content.ReadAsStringAsync();
            result = strJson.FromJson<T>();
        }
        catch (HttpRequestException)
        {
            throw;
        }
        catch (JsonException)
        {
            throw;
        }
        return result;
    }

    public void Dispose()
    {
    }
}