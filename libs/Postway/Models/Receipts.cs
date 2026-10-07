using System.Text.Json.Serialization;

namespace Postway.Models;

/// <summary>A price line on the public receipt (no cost fields).</summary>
public sealed class PublicReceiptPriceInfo
{
    /// <summary>Line description.</summary>
    [JsonPropertyName("description")]
    public string Description { get; set; } = "";

    /// <summary>Price in THB.</summary>
    [JsonPropertyName("price")]
    public decimal Price { get; set; }
}

/// <summary>A parcel on the public receipt (<c>PublicReceiptShipment</c>).</summary>
public sealed class PublicReceiptShipment
{
    /// <summary>Line number.</summary>
    [JsonPropertyName("seq")]
    public int Seq { get; set; }

    /// <summary>Courier name.</summary>
    [JsonPropertyName("shipment_provider_name")]
    public string ShipmentProviderName { get; set; } = "";

    /// <summary>Courier tracking number.</summary>
    [JsonPropertyName("tracking_no")]
    public string TrackingNo { get; set; } = "";

    /// <summary>Recipient name.</summary>
    [JsonPropertyName("recipient_fullname")]
    public string RecipientFullname { get; set; } = "";

    /// <summary>Recipient phone, masked.</summary>
    [JsonPropertyName("recipient_mobile_phone_masked")]
    public string RecipientMobilePhoneMasked { get; set; } = "";

    /// <summary>Recipient province.</summary>
    [JsonPropertyName("recipient_province")]
    public string RecipientProvince { get; set; } = "";

    /// <summary>Recipient postal code.</summary>
    [JsonPropertyName("recipient_zip_code")]
    public string RecipientZipCode { get; set; } = "";

    /// <summary>Weight in grams.</summary>
    [JsonPropertyName("weight")]
    public decimal Weight { get; set; }

    /// <summary>Width in cm.</summary>
    [JsonPropertyName("width")]
    public decimal Width { get; set; }

    /// <summary>Length in cm.</summary>
    [JsonPropertyName("length")]
    public decimal Length { get; set; }

    /// <summary>Height in cm.</summary>
    [JsonPropertyName("height")]
    public decimal Height { get; set; }

    /// <summary>COD amount in THB.</summary>
    [JsonPropertyName("cod")]
    public decimal Cod { get; set; }

    /// <summary>An <see cref="OrderShipmentStatus"/> value.</summary>
    [JsonPropertyName("status")]
    public string Status { get; set; } = "";

    /// <summary>Status, as display text.</summary>
    [JsonPropertyName("status_text")]
    public string StatusText { get; set; } = "";

    /// <summary>Creation time.</summary>
    [JsonPropertyName("created_at")]
    public DateTimeOffset? CreatedAt { get; set; }

    /// <summary>When the parcel went in transit.</summary>
    [JsonPropertyName("in_transit_at")]
    public DateTimeOffset? InTransitAt { get; set; }

    /// <summary>When the parcel was delivered.</summary>
    [JsonPropertyName("completed_at")]
    public DateTimeOffset? CompletedAt { get; set; }

    /// <summary>In-transit time, as display text.</summary>
    [JsonPropertyName("in_transit_at_text")]
    public string InTransitAtText { get; set; } = "";

    /// <summary>Delivery time, as display text.</summary>
    [JsonPropertyName("completed_at_text")]
    public string CompletedAtText { get; set; } = "";

    /// <summary>Price lines.</summary>
    [JsonPropertyName("price_infos")]
    public List<PublicReceiptPriceInfo> PriceInfos { get; set; } = [];
}

/// <summary><c>GET receipt/public/:token</c> response (<c>PublicReceiptResponse</c>).</summary>
public sealed class PublicReceiptResponse
{
    /// <summary><c>false</c> when the whole receipt was cancelled.</summary>
    [JsonPropertyName("is_available")]
    public bool IsAvailable { get; set; }

    /// <summary>Status message.</summary>
    [JsonPropertyName("message")]
    public string Message { get; set; } = "";

    /// <summary>Receipt number.</summary>
    [JsonPropertyName("no")]
    public string No { get; set; } = "";

    /// <summary>Formatted <c>yyyy/MM/dd HH:mm:ss</c> (not ISO), so kept as text.</summary>
    [JsonPropertyName("created_at")]
    public string CreatedAt { get; set; } = "";

    /// <summary>Store name.</summary>
    [JsonPropertyName("store_name")]
    public string StoreName { get; set; } = "";

    /// <summary>Display flag.</summary>
    [JsonPropertyName("is_show_public_receipt_store_name")]
    public bool IsShowPublicReceiptStoreName { get; set; }

    /// <summary>Display flag.</summary>
    [JsonPropertyName("is_show_public_receipt_header")]
    public bool IsShowPublicReceiptHeader { get; set; }

    /// <summary>Display flag.</summary>
    [JsonPropertyName("is_show_public_receipt_package_detail")]
    public bool IsShowPublicReceiptPackageDetail { get; set; }

    /// <summary>Display flag.</summary>
    [JsonPropertyName("is_show_public_receipt_total")]
    public bool IsShowPublicReceiptTotal { get; set; }

    /// <summary>Discount in THB.</summary>
    [JsonPropertyName("price_discount")]
    public decimal PriceDiscount { get; set; }

    /// <summary>Total in THB.</summary>
    [JsonPropertyName("price_total")]
    public decimal PriceTotal { get; set; }

    /// <summary>Amount collected in THB.</summary>
    [JsonPropertyName("price_get_total")]
    public decimal PriceGetTotal { get; set; }

    /// <summary>Amount charged in THB.</summary>
    [JsonPropertyName("price_charge_total")]
    public decimal PriceChargeTotal { get; set; }

    /// <summary>Parcels on the receipt.</summary>
    [JsonPropertyName("shipments")]
    public List<PublicReceiptShipment> Shipments { get; set; } = [];
}
