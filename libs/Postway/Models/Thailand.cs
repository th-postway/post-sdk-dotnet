using System.Text.Json.Serialization;

namespace Postway.Models;

/// <summary>
/// <c>POST thailand/filter</c> request (<c>MerchantThailandFilterRequest</c>). Field filters are exact matches;
/// <see cref="FilterRequest.Filter"/> matches any of sub-district / district / province / zipcode exactly.
/// </summary>
public sealed class MerchantThailandFilterRequest : FilterRequest
{
    /// <summary>Restrict to areas served by these couriers. Leave <c>null</c> (or empty) for all couriers; the SDK always sends the array.</summary>
    [JsonPropertyName("shipment_provider_names")]
    public List<string>? ShipmentProviderNames { get; set; }

    /// <summary>Exact sub-district.</summary>
    [JsonPropertyName("sub_district")]
    public string? SubDistrict { get; set; }

    /// <summary>Exact district.</summary>
    [JsonPropertyName("district")]
    public string? District { get; set; }

    /// <summary>Exact province.</summary>
    [JsonPropertyName("province")]
    public string? Province { get; set; }

    /// <summary>Exact postal code.</summary>
    [JsonPropertyName("zip_code")]
    public string? ZipCode { get; set; }
}

/// <summary>One postal area (<c>MerchantThailand</c>). Note the response spells it <c>zipcode</c>.</summary>
public sealed class MerchantThailand
{
    /// <summary>Couriers serving the area.</summary>
    [JsonPropertyName("shipment_provider_names")]
    public List<string> ShipmentProviderNames { get; set; } = [];

    /// <summary>Sub-district (tambon).</summary>
    [JsonPropertyName("sub_district")]
    public string SubDistrict { get; set; } = "";

    /// <summary>District (amphoe).</summary>
    [JsonPropertyName("district")]
    public string District { get; set; } = "";

    /// <summary>Province.</summary>
    [JsonPropertyName("province")]
    public string Province { get; set; } = "";

    /// <summary>Postal code (wire name <c>zipcode</c>).</summary>
    [JsonPropertyName("zipcode")]
    public string Zipcode { get; set; } = "";

    /// <summary>Island surcharge area.</summary>
    [JsonPropertyName("is_island")]
    public bool IsIsland { get; set; }

    /// <summary>Tourist surcharge area.</summary>
    [JsonPropertyName("is_tourist")]
    public bool IsTourist { get; set; }

    /// <summary>Remote-area surcharge area.</summary>
    [JsonPropertyName("is_remote_area")]
    public bool IsRemoteArea { get; set; }
}
