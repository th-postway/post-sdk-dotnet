using System.Text.Json.Serialization;

namespace Postway.Models;

/// <summary>A min/max range (<c>MerchantShipmentProviderDataWidth</c> / <c>…Height</c> / <c>…Length</c>).</summary>
public class MinMax
{
    /// <summary>Minimum.</summary>
    [JsonPropertyName("min")]
    public decimal Min { get; set; }

    /// <summary>Maximum.</summary>
    [JsonPropertyName("max")]
    public decimal Max { get; set; }
}

/// <summary>A range the courier prices on (<c>…Weight</c> / <c>…Cubic</c> / <c>…Dimension</c>).</summary>
public sealed class CalculatedRange : MinMax
{
    /// <summary>Whether the courier prices on this measure.</summary>
    [JsonPropertyName("is_calculate")]
    public bool IsCalculate { get; set; }
}

/// <summary>An optional service with limits (<c>…Cod</c> / <c>…Insurance</c>).</summary>
public sealed class EnabledRange : MinMax
{
    /// <summary>Whether the service is available.</summary>
    [JsonPropertyName("enable")]
    public bool Enable { get; set; }
}

/// <summary><c>MerchantShipmentProviderDataFee</c>.</summary>
public sealed class MerchantShipmentProviderFee
{
    /// <summary>COD fee, Postway to customer.</summary>
    [JsonPropertyName("cod_postway_to_customer")]
    public decimal CodPostwayToCustomer { get; set; }

    /// <summary>COD fee, customer to courier.</summary>
    [JsonPropertyName("cod_customer_to_mass")]
    public decimal CodCustomerToMass { get; set; }
}

/// <summary>A courier the store may ship with (<c>MerchantShipmentProviderData</c>).</summary>
public sealed class MerchantShipmentProviderData
{
    /// <summary>Use this value as <c>shipment_provider_name</c> / <c>shipment_name</c>.</summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = "";

    /// <summary>Human-readable name.</summary>
    [JsonPropertyName("display_name")]
    public string DisplayName { get; set; } = "";

    /// <summary>Width limits in cm.</summary>
    [JsonPropertyName("width")]
    public MinMax Width { get; set; } = new();

    /// <summary>Height limits in cm.</summary>
    [JsonPropertyName("height")]
    public MinMax Height { get; set; } = new();

    /// <summary>Length limits in cm.</summary>
    [JsonPropertyName("length")]
    public MinMax Length { get; set; } = new();

    /// <summary>Weight limits in grams.</summary>
    [JsonPropertyName("weight")]
    public CalculatedRange Weight { get; set; } = new();

    /// <summary>Cubic limits.</summary>
    [JsonPropertyName("cubic")]
    public CalculatedRange Cubic { get; set; } = new();

    /// <summary>Dimension (w + l + h) limits.</summary>
    [JsonPropertyName("dimension")]
    public CalculatedRange Dimension { get; set; } = new();

    /// <summary>COD availability and limits in THB.</summary>
    [JsonPropertyName("cod")]
    public EnabledRange Cod { get; set; } = new();

    /// <summary>Insurance availability and limits in THB.</summary>
    [JsonPropertyName("insurance")]
    public EnabledRange Insurance { get; set; } = new();

    /// <summary>Fees.</summary>
    [JsonPropertyName("fee")]
    public MerchantShipmentProviderFee Fee { get; set; } = new();
}
