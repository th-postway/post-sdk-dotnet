using System.Text.Json.Serialization;

namespace Postway.Models;

/// <summary><c>MerchantAuthAccountInfoResponseUser</c> — the store owner the session acts as.</summary>
public sealed class MerchantAccountUser
{
    /// <summary>Login name.</summary>
    [JsonPropertyName("username")]
    public string Username { get; set; } = "";

    /// <summary>E-mail address.</summary>
    [JsonPropertyName("email")]
    public string Email { get; set; } = "";

    /// <summary>First name.</summary>
    [JsonPropertyName("first_name")]
    public string FirstName { get; set; } = "";

    /// <summary>Last name.</summary>
    [JsonPropertyName("last_name")]
    public string LastName { get; set; } = "";

    /// <summary>Mobile phone number.</summary>
    [JsonPropertyName("mobile_phone")]
    public string MobilePhone { get; set; } = "";

    /// <summary>A <see cref="Models.UserRole"/> value.</summary>
    [JsonPropertyName("role")]
    public string Role { get; set; } = "";
}

/// <summary><c>MerchantAuthAccountInfoResponseStore</c>.</summary>
public sealed class MerchantAccountStore
{
    /// <summary>Store code.</summary>
    [JsonPropertyName("code")]
    public string Code { get; set; } = "";

    /// <summary>Store name.</summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = "";

    /// <summary>Store phone number.</summary>
    [JsonPropertyName("mobile_phone")]
    public string MobilePhone { get; set; } = "";

    /// <summary>A <see cref="Models.StoreBillingType"/> value.</summary>
    [JsonPropertyName("store_billing_type")]
    public string StoreBillingType { get; set; } = "";

    /// <summary>A <see cref="Models.StoreCodType"/> value.</summary>
    [JsonPropertyName("store_cod_type")]
    public string StoreCodType { get; set; } = "";

    /// <summary>Street address.</summary>
    [JsonPropertyName("address")]
    public string Address { get; set; } = "";

    /// <summary>Sub-district (tambon).</summary>
    [JsonPropertyName("sub_district")]
    public string SubDistrict { get; set; } = "";

    /// <summary>District (amphoe).</summary>
    [JsonPropertyName("district")]
    public string District { get; set; } = "";

    /// <summary>Province.</summary>
    [JsonPropertyName("province")]
    public string Province { get; set; } = "";

    /// <summary>Postal code.</summary>
    [JsonPropertyName("zip_code")]
    public string ZipCode { get; set; } = "";
}

/// <summary><c>MerchantAuthAccountInfoResponseSession</c>.</summary>
public sealed class MerchantAccountSession
{
    /// <summary>When the access token stops being accepted.</summary>
    [JsonPropertyName("expired")]
    public DateTimeOffset Expired { get; set; }
}

/// <summary><c>POST auth/account/info</c> response (<c>MerchantAuthAccountInfoResponse</c>).</summary>
public sealed class MerchantAuthAccountInfoResponse
{
    /// <summary>The store the token belongs to.</summary>
    [JsonPropertyName("store")]
    public MerchantAccountStore Store { get; set; } = new();

    /// <summary>The store owner the session acts as.</summary>
    [JsonPropertyName("user")]
    public MerchantAccountUser User { get; set; } = new();

    /// <summary>Session details, including expiry.</summary>
    [JsonPropertyName("session")]
    public MerchantAccountSession Session { get; set; } = new();
}
