using System.Text.Json;
using System.Text.Json.Nodes;
using Postway.Models;
using static Postway.UnitTest.Support.Stub;

namespace Postway.UnitTest.Core;

public class HttpPipelineTests
{
    [Fact]
    public async Task EncodesPathParameters()
    {
        var (client, handler) = Setup(Json("null"));
        await client.OrderShipments.GetByRefAsync("A/B ?#1");
        Assert.Equal($"{BaseUrl}/order-shipment/get-by-ref/A%2FB%20%3F%231", Only(handler).Url);
    }

    [Fact]
    public async Task EncodesThaiPathParameters()
    {
        var (client, handler) = Setup(Json("null"));
        await client.OrderShipments.GetByRefAsync("สีลม");
        Assert.Equal($"{BaseUrl}/order-shipment/get-by-ref/%E0%B8%AA%E0%B8%B5%E0%B8%A5%E0%B8%A1", Only(handler).Url);
    }

    [Fact]
    public async Task KeepsDotsInsideAPathParameter()
    {
        var (client, handler) = Setup([Json("{}")], o => o.AccessToken = null);
        await client.Receipts.GetPublicAsync("abc.def");
        Assert.Equal($"{BaseUrl}/receipt/public/abc.def", Only(handler).Url);
    }

    [Theory]
    [InlineData("")]
    [InlineData(".")]
    [InlineData("..")]
    [InlineData(null)]
    public async Task RejectsUnsafePathParametersBeforeAnyRequest(string? value)
    {
        var (client, handler) = Setup();
        await Assert.ThrowsAsync<PostwayConfigException>(() => client.OrderShipments.GetByRefAsync(value!));
        await Assert.ThrowsAsync<PostwayConfigException>(() => client.OrderShipments.GetByTrackingNoAsync(value!));
        await Assert.ThrowsAsync<PostwayConfigException>(() => client.Labels.ReceiptAsync(value!));
        await Assert.ThrowsAsync<PostwayConfigException>(() => client.Receipts.GetPublicAsync(value!));
        await Assert.ThrowsAsync<PostwayConfigException>(() => client.Receipts.GetPublicHtmlAsync(value!));
        Assert.Equal(0, handler.RequestCount);
    }

    [Fact]
    public void PathParameterErrorsNameTheParameterNotTheValue()
    {
        var (client, _) = Setup();
        var error = Assert.Throws<PostwayConfigException>(() => client.Receipts.PublicUrl(".."));
        Assert.Equal("token must be a non-empty string other than \".\" or \"..\"", error.Message);
    }

    [Fact]
    public async Task SetsContentTypeOnlyWhenThereIsABody()
    {
        var (client, handler) = Setup(Json("{}"), Json("[]"), Json("""{"count":0,"limit":1,"page":1,"page_count":0,"data":[]}"""));
        await client.Auth.AccountInfoAsync();
        await client.ShipmentProviders.AllAsync();
        await client.OrderShipments.FilterAsync(new MerchantOrderShipmentFilterRequest { Page = 1, Limit = 1 });
        Assert.False(handler.Calls[0].Headers.ContainsKey("Content-Type"));
        Assert.Null(handler.Calls[0].Body);
        Assert.False(handler.Calls[1].Headers.ContainsKey("Content-Type"));
        Assert.Equal("application/json", handler.Calls[2].Headers["Content-Type"]);
    }

    [Fact]
    public async Task SendsTheAcceptHeader()
    {
        var (client, handler) = Setup(Text("pong"));
        await client.Health.PingAsync();
        Assert.Equal("application/json, text/plain;q=0.9, */*;q=0.8", Only(handler).Headers["Accept"]);
    }

    [Theory]
    [InlineData(301)]
    [InlineData(302)]
    [InlineData(307)]
    [InlineData(308)]
    public async Task RefusesRedirectResponses(int status)
    {
        var redirect = Empty(status);
        redirect.Headers.Location = new Uri("https://elsewhere.example/steal");
        var (client, handler) = Setup(redirect);
        var error = await Assert.ThrowsAsync<PostwayRequestException>(() => client.Receipts.GetPublicAsync("secret-token"));
        Assert.Equal($"GET {BaseUrl}/receipt/public/:token failed: redirect refused", error.Message);
        Assert.Single(handler.Calls);
    }

    [Fact]
    public async Task RefusesAResponseThatAnInjectedHandlerReachedByFollowingARedirect()
    {
        var (client, handler) = Setup();
        handler.Enqueue((request, _) =>
        {
            request.RequestUri = new Uri("https://elsewhere.example/landing");
            return Task.FromResult(Json("{}"));
        });
        await Assert.ThrowsAsync<PostwayRequestException>(() => client.Auth.AccountInfoAsync());
    }

    [Fact]
    public async Task RefusesAGuardedCallWithoutAnAccessTokenBeforeAnyRequest()
    {
        var (client, handler) = Setup([], o => o.AccessToken = null);
        var error = await Assert.ThrowsAsync<PostwayConfigException>(() => client.Auth.AccountInfoAsync());
        Assert.StartsWith("POST auth/account/info requires a merchant access token", error.Message);
        Assert.Equal(0, handler.RequestCount);
    }

    [Fact]
    public async Task TreatsAnEmptyAccessTokenAsMissing()
    {
        var (client, handler) = Setup([], o => o.AccessToken = "");
        await Assert.ThrowsAsync<PostwayConfigException>(() => client.ShipmentProviders.AllAsync());
        Assert.Equal(0, handler.RequestCount);
    }

    [Theory]
    [InlineData(400, "ไม่พบคำสั่งซื้อ", 400)]
    [InlineData(403, "Forbidden resource", 400)]
    [InlineData(404, "not found", 400)]
    [InlineData(500, "เกิดข้อผิดพลาดในระบบ", 500)]
    public async Task MapsNon2xxToPostwayApiException(int status, string message, int code)
    {
        var (client, _) = Setup(ApiError(status, message));
        var error = await Assert.ThrowsAsync<PostwayApiException>(() => client.ShipmentProviders.AllAsync());
        Assert.IsNotType<PostwayBusinessException>(error);
        Assert.Equal(status, error.Status);
        Assert.Equal(code, error.Code);
        Assert.Equal([message], error.Messages);
        Assert.Equal(message, error.Message);
        Assert.Equal("GET", error.Method);
        Assert.Equal($"{BaseUrl}/shipment-provider/all", error.Url);
    }

    [Fact]
    public async Task KeepsEveryValidationMessage()
    {
        var (client, _) = Setup(ApiError(400, ["limit must not be less than 1", "page should not be empty"]));
        var error = await Assert.ThrowsAsync<PostwayApiException>(
            () => client.OrderShipments.FilterAsync(new MerchantOrderShipmentFilterRequest { Page = 0, Limit = 0 }));
        Assert.Equal(["limit must not be less than 1", "page should not be empty"], error.Messages);
        Assert.Equal("limit must not be less than 1; page should not be empty", error.Message);
    }

    [Fact]
    public async Task FallsBackToTheStatusWhenThereIsNoMessage()
    {
        var (client, _) = Setup(Empty(503));
        var error = await Assert.ThrowsAsync<PostwayApiException>(() => client.ShipmentProviders.AllAsync());
        Assert.Equal("HTTP 503", error.Message);
        Assert.Empty(error.Messages);
        Assert.Null(error.Body);
    }

    [Fact]
    public async Task HandlesANonJsonErrorBody()
    {
        var (client, _) = Setup(Text("<html>Bad Gateway</html>", 502));
        var error = await Assert.ThrowsAsync<PostwayApiException>(() => client.ShipmentProviders.AllAsync());
        Assert.Equal(502, error.Status);
        Assert.Equal("<html>Bad Gateway</html>", error.Body);
        Assert.Null(error.Code);
    }

    [Fact]
    public async Task KeepsTheResponseBodyReadableButOutOfMessagesAndSerialization()
    {
        const string body = """{"code":400,"isSuccess":false,"message":"ไม่พบคำสั่งซื้อ","data":null,"trace":"internal-detail"}""";
        var (client, _) = Setup(Json(body, 400));
        var error = await Assert.ThrowsAsync<PostwayApiException>(() => client.ShipmentProviders.AllAsync());
        Assert.Equal(body, error.Body);
        Assert.DoesNotContain("internal-detail", error.Message);
        Assert.DoesNotContain("internal-detail", error.ToString());
        Assert.Empty(error.Data);
        var json = JsonSerializer.Serialize(
            new { error.Method, error.Url, error.Status, error.Code, error.Messages, error.Message });
        Assert.DoesNotContain("internal-detail", json);
        Assert.NotNull(typeof(PostwayApiException).GetProperty(nameof(PostwayApiException.Body))!
            .GetCustomAttributes(typeof(System.Text.Json.Serialization.JsonIgnoreAttribute), false).SingleOrDefault());
    }

    [Fact]
    public async Task ReportsTheRouteTemplateInsteadOfTheReceiptTokenInApiErrors()
    {
        var (client, _) = Setup([ApiError(404, "ไม่พบใบเสร็จ")], o => o.AccessToken = null);
        var error = await Assert.ThrowsAsync<PostwayApiException>(() => client.Receipts.GetPublicAsync("secret-token"));
        Assert.Equal($"{BaseUrl}/receipt/public/:token", error.Url);
        Assert.DoesNotContain("secret-token", error.ToString());
    }

    [Fact]
    public async Task ReportsTheRouteTemplateForTrackingNumberLookups()
    {
        var (client, _) = Setup(ApiError(400, "ไม่พบคำสั่งซื้อ"));
        var error = await Assert.ThrowsAsync<PostwayApiException>(() => client.OrderShipments.GetByTrackingNoAsync("TH0001"));
        Assert.Equal($"{BaseUrl}/order-shipment/get-by-tracking-no/:tracking_no", error.Url);
        Assert.DoesNotContain("TH0001", error.ToString());
    }

    [Fact]
    public async Task TurnsA2xxEnvelopeWithIsSuccessFalseIntoPostwayBusinessException()
    {
        var (client, _) = Setup(Envelope(201, 400, false, JsonValue.Create("ยอดเงินไม่พอ"), null));
        var error = await Assert.ThrowsAsync<PostwayBusinessException>(() => client.OrderShipments.CancelAsync("TH1"));
        Assert.Equal(201, error.Status);
        Assert.Equal(400, error.Code);
        Assert.Equal(["ยอดเงินไม่พอ"], error.Messages);
        Assert.Equal($"{BaseUrl}/order-shipment/cancel", error.Url);
    }

    [Fact]
    public async Task RejectsA2xxResponseThatIsNotAnEnvelopeWhereOneIsExpected()
    {
        var (client, _) = Setup(Json("[1,2]"));
        var error = await Assert.ThrowsAsync<PostwayApiException>(() => client.OrderShipments.CancelAsync("TH1"));
        Assert.Contains("expected { code, isSuccess", error.Message);
    }

    [Fact]
    public async Task RejectsA2xxBodyOfTheWrongShape()
    {
        var (client, _) = Setup(Json("""{"count":"many"}"""));
        var error = await Assert.ThrowsAsync<PostwayApiException>(
            () => client.OrderShipments.FilterAsync(new MerchantOrderShipmentFilterRequest { Page = 1, Limit = 1 }));
        Assert.StartsWith("Unexpected response", error.Message);
        Assert.DoesNotContain("many", error.Message);
    }

    [Fact]
    public async Task WrapsNetworkFailuresInPostwayRequestExceptionWithTheCauseAndARedactedUrl()
    {
        var cause = new HttpRequestException("connection refused");
        var (client, handler) = Setup([], o => o.AccessToken = null);
        handler.Enqueue((_, _) => Task.FromException<HttpResponseMessage>(cause));
        var error = await Assert.ThrowsAsync<PostwayRequestException>(() => client.Receipts.GetPublicAsync("secret-token"));
        Assert.Same(cause, error.InnerException);
        Assert.Equal("GET", error.Method);
        Assert.Equal($"{BaseUrl}/receipt/public/:token", error.Url);
        Assert.Equal($"GET {BaseUrl}/receipt/public/:token failed: connection refused", error.Message);
    }

    [Fact]
    public async Task TimesOutWithPostwayRequestExceptionAndARedactedUrl()
    {
        var (client, handler) = Setup([], o => o.Timeout = TimeSpan.FromMilliseconds(20));
        handler.Enqueue(Hanging);
        var error = await Assert.ThrowsAsync<PostwayRequestException>(() => client.Receipts.GetPublicAsync("secret-token"));
        Assert.Equal($"GET {BaseUrl}/receipt/public/:token timed out after 20 ms", error.Message);
    }

    [Fact]
    public async Task HonoursAPerCallTimeout()
    {
        var (client, handler) = Setup();
        handler.Enqueue(Hanging);
        var error = await Assert.ThrowsAsync<PostwayRequestException>(
            () => client.Health.PingAsync(new RequestOptions { Timeout = TimeSpan.FromMilliseconds(15) }));
        Assert.EndsWith("timed out after 15 ms", error.Message);
    }

    [Fact]
    public async Task CallerCancellationThrowsOperationCanceledException()
    {
        var (client, handler) = Setup();
        handler.Enqueue(Hanging);
        using var source = new CancellationTokenSource();
        var pending = client.Health.PingAsync(new RequestOptions { Timeout = TimeSpan.FromSeconds(10) }, source.Token);
        await source.CancelAsync();
        var error = await Assert.ThrowsAnyAsync<OperationCanceledException>(() => pending);
        Assert.Equal(source.Token, error.CancellationToken);
        Assert.True(handler.LastToken.IsCancellationRequested);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(0.5)]
    [InlineData(1.5)]
    public async Task RejectsAnInvalidPerCallTimeoutBeforeAnyRequest(double milliseconds)
    {
        var (client, handler) = Setup();
        await Assert.ThrowsAsync<PostwayConfigException>(
            () => client.Health.PingAsync(new RequestOptions { Timeout = TimeSpan.FromMilliseconds(milliseconds) }));
        Assert.Equal(0, handler.RequestCount);
    }

    [Fact]
    public async Task SendsThaiTextAsUtf8Json()
    {
        var (client, handler) = Setup(Json("""{"count":0,"limit":1,"page":1,"page_count":0,"data":[]}"""));
        await client.OrderShipments.FilterAsync(new MerchantOrderShipmentFilterRequest { Filter = "สีลม", Page = 1, Limit = 1 });
        Assert.Contains("สีลม", Only(handler).RawBody);
    }

    [Fact]
    public async Task DoesNotRetry()
    {
        var (client, handler) = Setup(ApiError(500, "เกิดข้อผิดพลาดในระบบ"));
        await Assert.ThrowsAsync<PostwayApiException>(() => client.OrderShipments.CancelAsync("TH1"));
        Assert.Equal(1, handler.RequestCount);
    }
}
