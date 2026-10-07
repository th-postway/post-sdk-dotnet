using System.Net.Http.Headers;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Unicode;

namespace Postway;

/// <summary>Thin <see cref="HttpClient"/> wrapper: URL building, headers, timeout, body parsing and error mapping.</summary>
internal sealed class HttpPipeline : IDisposable
{
    public const string AcceptHeader = "application/json, text/plain;q=0.9, */*;q=0.8";

    /// <summary>Wire serialization: explicit <c>[JsonPropertyName]</c> on every model, unset optional fields left out.</summary>
    public static readonly JsonSerializerOptions JsonOptions = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
    };

    private readonly string? _accessToken;
    private readonly string _tokenType;
    private readonly TimeSpan _timeout;
    private readonly string _userAgent;
    private readonly HttpClient _http;
    private readonly bool _ownsHttp;

    /// <exception cref="PostwayConfigException">Unsafe base URL, header value or timeout; never echoes the value.</exception>
    public HttpPipeline(
        string baseUrl,
        string? accessToken,
        string tokenType,
        TimeSpan timeout,
        string userAgent,
        HttpClient? httpClient)
    {
        BaseUrl = Validation.NormalizeBaseUrl(baseUrl);
        if (accessToken is not null)
        {
            Validation.AssertHeaderValue(accessToken, "AccessToken");
        }

        Validation.AssertAuthScheme(tokenType);
        Validation.AssertHeaderValue(userAgent, "UserAgent");
        Validation.AssertTimeout(timeout, "Timeout");

        _accessToken = accessToken;
        _tokenType = tokenType;
        _timeout = timeout;
        _userAgent = userAgent;
        _ownsHttp = httpClient is null;
        _http = httpClient ?? CreateDefaultHttpClient();
    }

    /// <summary>Resolved base URL, without a trailing slash.</summary>
    public string BaseUrl { get; }

    /// <summary>Absolute URL for a call (segments validated and encoded, empty query values dropped).</summary>
    public string Url(IReadOnlyList<PathSegment> path, IReadOnlyList<KeyValuePair<string, string?>>? query = null)
    {
        var builder = new StringBuilder(BaseUrl);
        foreach (var segment in path)
        {
            Validation.AssertPathSegment(segment.Value, segment.Name ?? "path segment");
            builder.Append('/').Append(Uri.EscapeDataString(segment.Value));
        }

        var separator = '?';
        foreach (var (key, value) in query ?? [])
        {
            if (string.IsNullOrEmpty(value))
            {
                continue;
            }

            builder.Append(separator).Append(Uri.EscapeDataString(key)).Append('=').Append(Uri.EscapeDataString(value));
            separator = '&';
        }

        return builder.ToString();
    }

    /// <summary>Route template of a call, e.g. <c>receipt/public/:token</c>: static segments verbatim, params as <c>:name</c>.</summary>
    public static string Route(IReadOnlyList<PathSegment> path) =>
        string.Join('/', path.Select(segment => segment.Name is null ? segment.Value : $":{segment.Name}"));

    /// <summary><c>baseUrl/route</c>: the URL reported in errors, so caller-supplied values never reach logs.</summary>
    public string RouteUrl(IReadOnlyList<PathSegment> path) => $"{BaseUrl}/{Route(path)}";

    /// <summary>Perform the call and deserialize the JSON body (<c>default</c> for an empty body or JSON <c>null</c>).</summary>
    public async Task<T?> RequestAsync<T>(HttpCall call, CancellationToken cancellationToken)
    {
        var result = await SendCheckedAsync(call, cancellationToken).ConfigureAwait(false);
        if (result.Body is null)
        {
            return default;
        }

        if (!result.IsJson)
        {
            throw Unexpected(call, result, "Unexpected response: expected a JSON body");
        }

        try
        {
            return JsonSerializer.Deserialize<T>(result.Body, JsonOptions);
        }
        catch (JsonException error)
        {
            throw Unexpected(call, result, "Unexpected response: the JSON body does not match the expected shape", error);
        }
    }

    /// <summary>Perform the call and return the body as text: a JSON string is unwrapped, other JSON is returned raw, empty is <c>""</c>.</summary>
    public async Task<string> RequestTextAsync(HttpCall call, CancellationToken cancellationToken)
    {
        var result = await SendCheckedAsync(call, cancellationToken).ConfigureAwait(false);
        if (result.Body is null)
        {
            return "";
        }

        if (result.IsJson && TryParse(result.Body, out var document))
        {
            using (document)
            {
                return document.RootElement.ValueKind == JsonValueKind.String
                    ? document.RootElement.GetString()!
                    : result.Body;
            }
        }

        return result.Body;
    }

    /// <summary>
    /// Perform a call whose response is an <c>HttpBaseResponse&lt;T&gt;</c> envelope and return <c>data</c>.
    /// A 2xx envelope with <c>isSuccess: false</c> throws <see cref="PostwayBusinessException"/>.
    /// </summary>
    public async Task<T?> RequestEnvelopeAsync<T>(HttpCall call, CancellationToken cancellationToken)
    {
        var result = await SendCheckedAsync(call, cancellationToken).ConfigureAwait(false);
        JsonDocument? document = null;
        if (result.Body is null || !result.IsJson || !TryParse(result.Body, out document)
            || document.RootElement.ValueKind != JsonValueKind.Object
            || !document.RootElement.TryGetProperty("isSuccess", out var isSuccess))
        {
            document?.Dispose();
            throw Unexpected(call, result, "Unexpected response: expected { code, isSuccess, message, data }");
        }

        using (document)
        {
            if (isSuccess.ValueKind == JsonValueKind.False)
            {
                var (code, messages) = Describe(result);
                throw new PostwayBusinessException(call.Method.Method, result.Url, result.Status, code, messages, result.Body);
            }

            if (!document.RootElement.TryGetProperty("data", out var data))
            {
                return default;
            }

            try
            {
                return data.Deserialize<T>(JsonOptions);
            }
            catch (JsonException error)
            {
                throw Unexpected(call, result, "Unexpected response: the envelope data does not match the expected shape", error);
            }
        }
    }

    public void Dispose()
    {
        if (_ownsHttp)
        {
            _http.Dispose();
        }
    }

    private static HttpClient CreateDefaultHttpClient() =>
        new(new SocketsHttpHandler
        {
            AllowAutoRedirect = false,
            PooledConnectionLifetime = TimeSpan.FromMinutes(5),
            AutomaticDecompression = System.Net.DecompressionMethods.All,
        }, disposeHandler: true)
        {
            Timeout = Timeout.InfiniteTimeSpan,
        };

    /// <summary><see cref="SendAsync"/>, throwing <see cref="PostwayApiException"/> for a non-2xx status.</summary>
    private async Task<SendResult> SendCheckedAsync(HttpCall call, CancellationToken cancellationToken)
    {
        var result = await SendAsync(call, cancellationToken).ConfigureAwait(false);
        if (result.Status is < 200 or >= 300)
        {
            var (code, messages) = Describe(result);
            throw new PostwayApiException(call.Method.Method, result.Url, result.Status, code, messages, result.Body);
        }

        return result;
    }

    /// <summary>The returned <c>Url</c> is the redacted route URL, not the one sent.</summary>
    private async Task<SendResult> SendAsync(HttpCall call, CancellationToken cancellationToken)
    {
        var method = call.Method.Method;
        var url = new Uri(Url(call.Path, call.Query));
        var reportedUrl = RouteUrl(call.Path);

        using var request = new HttpRequestMessage(call.Method, url);
        request.Headers.TryAddWithoutValidation("Accept", AcceptHeader);
        request.Headers.TryAddWithoutValidation("User-Agent", _userAgent);
        if (call.Auth)
        {
            if (_accessToken is null)
            {
                throw new PostwayConfigException(
                    $"{method} {Route(call.Path)} requires a merchant access token; set AccessToken on the client options");
            }

            request.Headers.TryAddWithoutValidation("Authorization", $"{_tokenType} {_accessToken}");
        }

        if (call.Body is not null)
        {
            var json = JsonSerializer.Serialize(call.Body, call.Body.GetType(), JsonOptions);
            request.Content = new StringContent(json, Encoding.UTF8);
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        }

        var timeout = call.Options?.Timeout ?? _timeout;
        if (call.Options?.Timeout is { } perCall)
        {
            Validation.AssertTimeout(perCall, "Timeout");
        }

        using var timeoutSource = new CancellationTokenSource(timeout);
        using var linked = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutSource.Token);
        try
        {
            using var response = await _http
                .SendAsync(request, HttpCompletionOption.ResponseHeadersRead, linked.Token)
                .ConfigureAwait(false);

            var status = (int)response.StatusCode;
            if (status is >= 300 and < 400 || response.RequestMessage?.RequestUri is { } finalUrl && finalUrl != url)
            {
                throw new PostwayRequestException(method, reportedUrl, $"{method} {reportedUrl} failed: redirect refused");
            }

            var text = await response.Content.ReadAsStringAsync(linked.Token).ConfigureAwait(false);
            var mediaType = response.Content.Headers.ContentType?.MediaType;
            return new SendResult(
                status,
                text.Length == 0 ? null : text,
                mediaType is not null && mediaType.Contains("json", StringComparison.OrdinalIgnoreCase),
                reportedUrl);
        }
        catch (PostwayException)
        {
            throw;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            cancellationToken.ThrowIfCancellationRequested();
            throw;
        }
        catch (OperationCanceledException error) when (timeoutSource.IsCancellationRequested)
        {
            throw new PostwayRequestException(
                method,
                reportedUrl,
                $"{method} {reportedUrl} timed out after {(long)timeout.TotalMilliseconds} ms",
                error);
        }
        catch (Exception error)
        {
            throw new PostwayRequestException(method, reportedUrl, $"{method} {reportedUrl} failed: {error.Message}", error);
        }
    }

    private static PostwayApiException Unexpected(HttpCall call, SendResult result, string message, Exception? inner = null)
    {
        return new PostwayApiException(call.Method.Method, result.Url, result.Status, null, [], result.Body, message, inner);
    }

    private static bool TryParse(string text, [System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out JsonDocument? document)
    {
        try
        {
            document = JsonDocument.Parse(text);
            return true;
        }
        catch (JsonException)
        {
            document = null;
            return false;
        }
    }

    /// <summary>Body <c>code</c> (when a number) and <c>message</c> (string or list) of a response.</summary>
    private static (int? Code, IReadOnlyList<string> Messages) Describe(SendResult result)
    {
        if (result.Body is null)
        {
            return (null, []);
        }

        if (!result.IsJson || !TryParse(result.Body, out var document))
        {
            return (null, [result.Body]);
        }

        using (document)
        {
            var root = document.RootElement;
            switch (root.ValueKind)
            {
                case JsonValueKind.String:
                    var text = root.GetString();
                    return (null, string.IsNullOrEmpty(text) ? [] : [text]);
                case JsonValueKind.Object:
                    int? code = root.TryGetProperty("code", out var codeElement)
                        && codeElement.ValueKind == JsonValueKind.Number
                        && codeElement.TryGetInt32(out var number)
                            ? number
                            : null;
                    IReadOnlyList<string> messages = [];
                    if (root.TryGetProperty("message", out var message))
                    {
                        if (message.ValueKind == JsonValueKind.Array)
                        {
                            messages = message
                                .EnumerateArray()
                                .Select(item => item.ValueKind == JsonValueKind.String ? item.GetString()! : item.GetRawText())
                                .ToArray();
                        }
                        else if (message.ValueKind == JsonValueKind.String && message.GetString() is { Length: > 0 } single)
                        {
                            messages = [single];
                        }
                    }

                    return (code, messages);
                default:
                    return (null, []);
            }
        }
    }

    private readonly record struct SendResult(int Status, string? Body, bool IsJson, string Url);
}
