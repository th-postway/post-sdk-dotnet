using static Postway.UnitTest.Support.Stub;

namespace Postway.UnitTest.Core;

public class ClientTests
{
    private static readonly HttpClient Unused = new(new Support.StubHandler());

    /// <summary>Construct with <paramref name="configure"/> and return the <see cref="PostwayConfigException"/> it throws.</summary>
    private static PostwayConfigException ConfigError(Action<PostwayMerchantClientOptions> configure)
    {
        var options = new PostwayMerchantClientOptions { HttpClient = Unused };
        configure(options);
        return Assert.Throws<PostwayConfigException>(() => new PostwayMerchantClient(options));
    }

    [Fact]
    public void DefaultsToTheProductionBaseUrl()
    {
        using var client = new PostwayMerchantClient();
        Assert.Equal("https://post.postway.co.th/merchant", client.BaseUrl);
    }

    [Theory]
    [InlineData(MerchantEnvironment.Production, "https://post.postway.co.th/merchant")]
    [InlineData(MerchantEnvironment.Sandbox, "https://sandbox-post.postway.co.th/merchant")]
    public void ResolvesEnvironments(MerchantEnvironment environment, string url)
    {
        using var client = new PostwayMerchantClient(new PostwayMerchantClientOptions { Environment = environment });
        Assert.Equal(url, client.BaseUrl);
        Assert.Equal(url, MerchantBaseUrls.For(environment));
    }

    [Fact]
    public void AcceptsAnAccessTokenShorthand()
    {
        using var client = new PostwayMerchantClient("tok_123");
        Assert.Equal(MerchantBaseUrls.Production, client.BaseUrl);
    }

    [Fact]
    public void LetsAnExplicitBaseUrlWinOverEnvironment()
    {
        using var client = new PostwayMerchantClient(new PostwayMerchantClientOptions
        {
            BaseUrl = "https://merchant.example/v1",
            Environment = MerchantEnvironment.Sandbox,
        });
        Assert.Equal("https://merchant.example/v1", client.BaseUrl);
    }

    [Theory]
    [InlineData("http://localhost:3000/api//", "http://localhost:3000/api")]
    [InlineData("http://127.0.0.1:3000/api", "http://127.0.0.1:3000/api")]
    [InlineData("http://[::1]:3000/api", "http://[::1]:3000/api")]
    [InlineData("HTTPS://Merchant.Example/merchant/", "https://merchant.example/merchant")]
    [InlineData("https://merchant.example", "https://merchant.example")]
    [InlineData("https://merchant.example:8443/merchant", "https://merchant.example:8443/merchant")]
    public void AcceptsAndNormalisesBaseUrls(string input, string expected)
    {
        using var client = new PostwayMerchantClient(new PostwayMerchantClientOptions { BaseUrl = input });
        Assert.Equal(expected, client.BaseUrl);
    }

    [Theory]
    [InlineData("http://merchant.example/merchant", "merchant.example")]
    [InlineData("ftp://merchant.example/merchant", "ftp:")]
    [InlineData("file:///etc/passwd", "passwd")]
    [InlineData("/merchant", "/merchant")]
    [InlineData("not a url", "not a url")]
    [InlineData("", "")]
    [InlineData("https://user:s3cret@merchant.example/merchant", "s3cret")]
    [InlineData("https://merchant.example/merchant?debug=1", "debug")]
    [InlineData("https://merchant.example/merchant?", "merchant.example")]
    [InlineData("https://merchant.example/merchant#section-9", "section-9")]
    public void RejectsUnsafeBaseUrlsWithoutEchoingThem(string baseUrl, string secret)
    {
        var error = ConfigError(o => o.BaseUrl = baseUrl);
        if (secret.Length > 0)
        {
            Assert.DoesNotContain(secret, error.Message);
        }
    }

    [Fact]
    public void RejectsAnUnknownEnvironmentNamingTheValidOnesButNotTheInput()
    {
        var error = ConfigError(o => o.Environment = (MerchantEnvironment)42);
        Assert.Contains("Production, Sandbox", error.Message);
        Assert.DoesNotContain("42", error.Message);
    }

    public static TheoryData<string, Action<PostwayMerchantClientOptions>, string> HeaderInjection => new()
    {
        { "AccessToken with CRLF", o => o.AccessToken = "tok\r\nX-Injected: 1", "X-Injected" },
        { "AccessToken with a trailing newline", o => o.AccessToken = "tok_secret\n", "tok_secret" },
        { "TokenType with a newline", o => o.TokenType = "Bearer\n", "Bearer\n" },
        { "UserAgent with a trailing newline", o => o.UserAgent = "ua/1\n", "ua/1" },
        { "TokenType with a space", o => o.TokenType = "Bearer x", "Bearer x" },
        { "TokenType empty", o => o.TokenType = "", "TokenType must" },
        { "UserAgent with a NUL byte", o => o.UserAgent = "ua\0", "ua\0" },
        { "non-ASCII UserAgent", o => o.UserAgent = "ร้าน/1.0", "ร้าน" },
        { "empty UserAgent", o => o.UserAgent = "", "\"\"" },
    };

    [Theory]
    [MemberData(nameof(HeaderInjection))]
    public void RejectsHeaderInjectionAtConstruction(string label, Action<PostwayMerchantClientOptions> configure, string secret)
    {
        _ = label;
        var error = ConfigError(configure);
        if (secret != "TokenType must")
        {
            Assert.DoesNotContain(secret, error.Message);
        }
    }

    public static TheoryData<TimeSpan> BadTimeouts => new()
    {
        TimeSpan.Zero,
        TimeSpan.FromMilliseconds(-5),
        TimeSpan.FromTicks(15_000),
        TimeSpan.FromMilliseconds(2_147_483_648),
        Timeout.InfiniteTimeSpan,
        TimeSpan.MaxValue,
    };

    [Theory]
    [MemberData(nameof(BadTimeouts))]
    public void RejectsBadTimeouts(TimeSpan timeout)
    {
        var error = ConfigError(o => o.Timeout = timeout);
        Assert.StartsWith("Timeout must be a positive whole number of milliseconds", error.Message);
        Assert.DoesNotContain("1.5", error.Message);
    }

    [Fact]
    public async Task UsesTheConfiguredTokenTypeAndUserAgent()
    {
        var (client, handler) = Setup([Json("{}")], o =>
        {
            o.TokenType = "Custom";
            o.UserAgent = "my-shop/1.0";
        });
        await client.Auth.AccountInfoAsync();
        var call = Only(handler);
        Assert.Equal("Custom tok_123", call.Headers["Authorization"]);
        Assert.Equal("my-shop/1.0", call.Headers["User-Agent"]);
    }

    [Fact]
    public async Task SendsADefaultUserAgentNamingTheSdkAndRuntimeVersions()
    {
        var (client, handler) = Setup(Json("{}"));
        await client.Auth.AccountInfoAsync();
        Assert.Matches(@"^postway-sdk-dotnet/\d+\.\d+\.\d+ dotnet/\d+\.\d+", Only(handler).Headers["User-Agent"]);
    }

    [Fact]
    public async Task DoesNotDisposeAnInjectedHttpClient()
    {
        var handler = new Support.StubHandler();
        var http = new HttpClient(handler);
        new PostwayMerchantClient(new PostwayMerchantClientOptions { HttpClient = http }).Dispose();
        handler.Enqueue((_, _) => Task.FromResult(Text("pong")));
        Assert.Equal("pong", await http.GetStringAsync(new Uri("https://merchant.test/x")));
    }
}
