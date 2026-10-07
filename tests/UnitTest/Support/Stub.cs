using System.Net;
using System.Text;
using System.Text.Json.Nodes;

namespace Postway.UnitTest.Support;

/// <summary>One request as the stub saw it.</summary>
public sealed record RecordedCall(string Method, string Url, IReadOnlyDictionary<string, string> Headers, JsonNode? Body, string? RawBody);

/// <summary>Answers each request with the next queued responder and records what was sent. Safe for concurrent requests.</summary>
public sealed class StubHandler : HttpMessageHandler
{
    private readonly Queue<Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>>> _queue = new();
    private readonly object _gate = new();

    public List<RecordedCall> Calls { get; } = [];

    public int RequestCount { get; private set; }

    public CancellationToken LastToken { get; private set; }

    public void Enqueue(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> responder)
    {
        lock (_gate)
        {
            _queue.Enqueue(responder);
        }
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var (name, values) in request.Headers.NonValidated)
        {
            headers[name] = string.Join(", ", values);
        }

        string? raw = null;
        if (request.Content is not null)
        {
            foreach (var (name, values) in request.Content.Headers.NonValidated)
            {
                headers[name] = string.Join(", ", values);
            }

            raw = await request.Content.ReadAsStringAsync(cancellationToken);
        }

        var call = new RecordedCall(
            request.Method.Method,
            request.RequestUri!.AbsoluteUri,
            headers,
            raw is null ? null : JsonNode.Parse(raw),
            raw);

        Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>>? responder;
        lock (_gate)
        {
            RequestCount++;
            LastToken = cancellationToken;
            Calls.Add(call);
            if (!_queue.TryDequeue(out responder))
            {
                throw new InvalidOperationException($"unexpected request {request.Method} {request.RequestUri}");
            }
        }

        var response = await responder(request, cancellationToken);
        response.RequestMessage ??= request;
        return response;
    }
}

/// <summary>Response builders and a client wired to <see cref="StubHandler"/>; the .NET twin of <c>mock-fetch.ts</c>.</summary>
public static class Stub
{
    public const string BaseUrl = "https://merchant.test/merchant";
    public const string Token = "tok_123";
    public const string Auth = $"Bearer {Token}";

    /// <summary>JSON response as the API sends it.</summary>
    public static HttpResponseMessage Json(string json, int status = 200) =>
        new((HttpStatusCode)status) { Content = new StringContent(json, Encoding.UTF8, "application/json") };

    public static HttpResponseMessage Json(JsonNode? body, int status = 200) => Json(body?.ToJsonString() ?? "null", status);

    /// <summary>Empty body, which the API sends when a route returns <c>null</c>.</summary>
    public static HttpResponseMessage Empty(int status = 200) => new((HttpStatusCode)status) { Content = new ByteArrayContent([]) };

    public static HttpResponseMessage Text(string body, int status = 200, string mediaType = "text/html") =>
        new((HttpStatusCode)status) { Content = new StringContent(body, Encoding.UTF8, mediaType) };

    /// <summary>The API's error envelope.</summary>
    public static HttpResponseMessage ApiError(int status, string message, int? code = null) =>
        Envelope(status, code ?? (status == 500 ? 500 : 400), false, JsonValue.Create(message), null);

    public static HttpResponseMessage ApiError(int status, string[] messages, int? code = null) =>
        Envelope(status, code ?? (status == 500 ? 500 : 400), false, new JsonArray([.. messages.Select(m => JsonValue.Create(m))]), null);

    public static HttpResponseMessage Envelope(int status, int code, bool isSuccess, JsonNode? message, JsonNode? data) =>
        Json(new JsonObject { ["code"] = code, ["isSuccess"] = isSuccess, ["message"] = message, ["data"] = data }, status);

    /// <summary>A client whose stub answers with <paramref name="responses"/> in order.</summary>
    public static (PostwayMerchantClient Client, StubHandler Handler) Setup(
        IEnumerable<HttpResponseMessage>? responses = null,
        Action<PostwayMerchantClientOptions>? configure = null)
    {
        var handler = new StubHandler();
        foreach (var response in responses ?? [])
        {
            handler.Enqueue((_, _) => Task.FromResult(response));
        }

        var options = new PostwayMerchantClientOptions
        {
            BaseUrl = BaseUrl,
            AccessToken = Token,
            HttpClient = new HttpClient(handler),
        };
        configure?.Invoke(options);
        return (new PostwayMerchantClient(options), handler);
    }

    public static (PostwayMerchantClient Client, StubHandler Handler) Setup(params HttpResponseMessage[] responses) =>
        Setup(responses, null);

    /// <summary>The single recorded call; fails the test otherwise.</summary>
    public static RecordedCall Only(StubHandler handler)
    {
        Assert.Single(handler.Calls);
        return handler.Calls[0];
    }

    /// <summary>A responder that never answers and fails once the token fires.</summary>
    public static async Task<HttpResponseMessage> Hanging(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        _ = request;
        await Task.Delay(Timeout.Infinite, cancellationToken);
        throw new InvalidOperationException("unreachable");
    }

    /// <summary>Asserts two JSON documents are structurally equal.</summary>
    public static void JsonEqual(string expected, JsonNode? actual) =>
        Assert.True(JsonNode.DeepEquals(JsonNode.Parse(expected), actual), $"expected {expected}\nactual   {actual?.ToJsonString()}");
}
