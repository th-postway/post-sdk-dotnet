using static Postway.UnitTest.Support.Stub;

namespace Postway.UnitTest.Resources;

public class ReceiptsTests
{
    [Fact]
    public async Task GetPublicGetsWithoutAuthorization()
    {
        var (client, handler) = Setup(
            [Json("""{"is_available":true,"no":"RC-001","created_at":"2026/10/06 10:00:00","shipments":[]}""")],
            o => o.AccessToken = null);
        var receipt = await client.Receipts.GetPublicAsync("abc.def");
        Assert.True(receipt.IsAvailable);
        Assert.Equal("RC-001", receipt.No);
        Assert.Equal("2026/10/06 10:00:00", receipt.CreatedAt);
        var call = Only(handler);
        Assert.Equal("GET", call.Method);
        Assert.Equal($"{BaseUrl}/receipt/public/abc.def", call.Url);
        Assert.False(call.Headers.ContainsKey("Authorization"));
    }

    [Fact]
    public async Task GetPublicNeverSendsTheTokenEvenWhenTheClientHasOne()
    {
        var (client, handler) = Setup(Json("{}"));
        await client.Receipts.GetPublicAsync("abc");
        Assert.False(Only(handler).Headers.ContainsKey("Authorization"));
    }

    [Fact]
    public async Task GetPublicThrowsPostwayApiException404ForABadToken()
    {
        var (client, _) = Setup(Envelope(404, 400, false, System.Text.Json.Nodes.JsonValue.Create("ไม่พบใบเสร็จ"), null));
        var error = await Assert.ThrowsAsync<PostwayApiException>(() => client.Receipts.GetPublicAsync("bad"));
        Assert.Equal(404, error.Status);
    }

    [Fact]
    public async Task GetPublicHtmlReturnsTextAndA404PageBecomesPostwayApiExceptionWithTheHtmlBody()
    {
        var (client, handler) = Setup(Text("<html>ok</html>"), Text("<html>ไม่พบ</html>", 404));
        Assert.Equal("<html>ok</html>", await client.Receipts.GetPublicHtmlAsync("tok"));
        var error = await Assert.ThrowsAsync<PostwayApiException>(() => client.Receipts.GetPublicHtmlAsync("bad"));
        Assert.Equal("<html>ไม่พบ</html>", error.Body);
        Assert.Equal("GET", handler.Calls[0].Method);
        Assert.Equal($"{BaseUrl}/receipt/tok", handler.Calls[0].Url);
        Assert.False(handler.Calls[0].Headers.ContainsKey("Authorization"));
    }

    [Fact]
    public void PublicUrlBuildsTheQrLinkWithoutARequest()
    {
        var (client, handler) = Setup();
        Assert.Equal($"{BaseUrl}/receipt/a%2Fb", client.Receipts.PublicUrl("a/b"));
        Assert.Equal(0, handler.RequestCount);
    }
}
