using static Postway.UnitTest.Support.Stub;

namespace Postway.UnitTest.Resources;

public class HealthTests
{
    [Fact]
    public async Task PingGetsWithoutAuthorization()
    {
        var (client, handler) = Setup(Text("pong", mediaType: "text/plain"));
        Assert.Equal("pong", await client.Health.PingAsync());
        var call = Only(handler);
        Assert.Equal("GET", call.Method);
        Assert.Equal($"{BaseUrl}/health/ping", call.Url);
        Assert.False(call.Headers.ContainsKey("Authorization"));
        Assert.Null(call.Body);
    }

    [Theory]
    [InlineData("\"pong\"", "pong")]
    [InlineData("{\"status\":\"ok\"}", "{\"status\":\"ok\"}")]
    public async Task PingReturnsJsonBodiesAsText(string body, string expected)
    {
        var (client, _) = Setup(Json(body));
        Assert.Equal(expected, await client.Health.PingAsync());
    }

    [Fact]
    public async Task PingReturnsEmptyTextForAnEmptyBody()
    {
        var (client, _) = Setup(Empty());
        Assert.Equal("", await client.Health.PingAsync());
    }
}
