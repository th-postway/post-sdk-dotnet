namespace Postway;

/// <summary>Named Postway environments. Set <see cref="PostwayMerchantClientOptions.BaseUrl"/> for any other host.</summary>
public enum MerchantEnvironment
{
    /// <summary><c>https://post.postway.co.th/merchant</c> (default).</summary>
    Production,

    /// <summary><c>https://sandbox-post.postway.co.th/merchant</c>, for integration testing.</summary>
    Sandbox,
}

/// <summary>Public Merchant API base URLs.</summary>
public static class MerchantBaseUrls
{
    /// <summary>Production base URL.</summary>
    public const string Production = "https://post.postway.co.th/merchant";

    /// <summary>Sandbox base URL, for integration testing.</summary>
    public const string Sandbox = "https://sandbox-post.postway.co.th/merchant";

    /// <summary>The base URL of <paramref name="environment"/>.</summary>
    /// <exception cref="PostwayConfigException">The value is not a defined environment.</exception>
    public static string For(MerchantEnvironment environment) =>
        environment switch
        {
            MerchantEnvironment.Production => Production,
            MerchantEnvironment.Sandbox => Sandbox,
            _ => throw new PostwayConfigException("Environment must be one of: Production, Sandbox (or set BaseUrl)"),
        };
}
