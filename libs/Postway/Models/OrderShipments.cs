using System.Text.Json.Serialization;

namespace Postway.Models;

// ---------------------------------------------------------------- read model

/// <summary><c>MerchantOrderShipmentDataSender</c> / <c>…Recipient</c>.</summary>
public sealed class MerchantOrderShipmentParty
{
    /// <summary>Full name.</summary>
    [JsonPropertyName("fullname")]
    public string Fullname { get; set; } = "";

    /// <summary>Mobile phone number.</summary>
    [JsonPropertyName("mobile_phone")]
    public string MobilePhone { get; set; } = "";

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

/// <summary><c>MerchantOrderShipmentDataPackage</c> — dimensions in cm, weight in grams.</summary>
public sealed class MerchantOrderShipmentDataPackage
{
    /// <summary>Width in cm.</summary>
    [JsonPropertyName("width")]
    public decimal Width { get; set; }

    /// <summary>Length in cm.</summary>
    [JsonPropertyName("length")]
    public decimal Length { get; set; }

    /// <summary>Height in cm.</summary>
    [JsonPropertyName("height")]
    public decimal Height { get; set; }

    /// <summary>Weight in grams.</summary>
    [JsonPropertyName("weight")]
    public decimal Weight { get; set; }

    /// <summary>The merchant's own reference given at create time.</summary>
    [JsonPropertyName("my_tracking_no")]
    public string MyTrackingNo { get; set; } = "";

    /// <summary>Courier tracking number.</summary>
    [JsonPropertyName("tracking_no")]
    public string TrackingNo { get; set; } = "";

    /// <summary>Reference 1.</summary>
    [JsonPropertyName("ref1")]
    public string Ref1 { get; set; } = "";

    /// <summary>Reference 2.</summary>
    [JsonPropertyName("ref2")]
    public string Ref2 { get; set; } = "";

    /// <summary>Reference 3.</summary>
    [JsonPropertyName("ref3")]
    public string Ref3 { get; set; } = "";
}

/// <summary>One parcel as returned by every order-shipment read (<c>MerchantOrderShipmentData</c>).</summary>
public sealed class MerchantOrderShipmentData
{
    /// <summary>An <see cref="OrderShipmentChannel"/> value.</summary>
    [JsonPropertyName("channel")]
    public string Channel { get; set; } = "";

    /// <summary>Sender.</summary>
    [JsonPropertyName("sender")]
    public MerchantOrderShipmentParty Sender { get; set; } = new();

    /// <summary>Recipient.</summary>
    [JsonPropertyName("recipient")]
    public MerchantOrderShipmentParty Recipient { get; set; } = new();

    /// <summary>Package dimensions and references.</summary>
    [JsonPropertyName("package")]
    public MerchantOrderShipmentDataPackage Package { get; set; } = new();

    /// <summary>An <see cref="Models.OrderStatus"/> value.</summary>
    [JsonPropertyName("status")]
    public string Status { get; set; } = "";

    /// <summary>An <see cref="Models.OrderShipmentStatus"/> value.</summary>
    [JsonPropertyName("order_shipment_status")]
    public string OrderShipmentStatus { get; set; } = "";

    /// <summary>Creation time.</summary>
    [JsonPropertyName("created_at")]
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>Last update time.</summary>
    [JsonPropertyName("updated_at")]
    public DateTimeOffset? UpdatedAt { get; set; }

    /// <summary>When the parcel went in transit.</summary>
    [JsonPropertyName("in_transit_at")]
    public DateTimeOffset? InTransitAt { get; set; }

    /// <summary>When the parcel was delivered.</summary>
    [JsonPropertyName("completed_at")]
    public DateTimeOffset? CompletedAt { get; set; }
}

/// <summary>
/// <c>POST order-shipment/filter</c> request. <see cref="FilterRequest.Filter"/> is a case-insensitive match against
/// sender and recipient name/phone/address fields, <c>my_tracking_no</c>, <c>tracking_no</c> and <c>ref1..3</c>.
/// </summary>
public sealed class MerchantOrderShipmentFilterRequest : FilterRequest { }

// ---------------------------------------------------------------- create

/// <summary><c>MerchantOrderShipmentCreateRequestShipping</c>.</summary>
public sealed class MerchantOrderShipmentCreateShipping
{
    /// <summary>A <c>name</c> from <c>ShipmentProviders.AllAsync()</c>, e.g. <c>"Flash"</c>.</summary>
    [JsonPropertyName("shipment_provider_name")]
    public required string ShipmentProviderName { get; set; }

    /// <summary>Your own reference for the parcel.</summary>
    [JsonPropertyName("my_tracking_no")]
    public string? MyTrackingNo { get; set; }

    /// <summary>Only for couriers where you already hold a tracking number.</summary>
    [JsonPropertyName("shipment_provider_tracking_no")]
    public string? ShipmentProviderTrackingNo { get; set; }
}

/// <summary><c>MerchantOrderShipmentCreateRequestSender</c>.</summary>
public sealed class MerchantOrderShipmentCreateSender
{
    /// <summary>Full name.</summary>
    [JsonPropertyName("fullname")]
    public required string Fullname { get; set; }

    /// <summary>E-mail address.</summary>
    [JsonPropertyName("email")]
    public string? Email { get; set; }

    /// <summary>Mobile phone number.</summary>
    [JsonPropertyName("mobile_phone")]
    public required string MobilePhone { get; set; }

    /// <summary>National ID / passport number, when the courier requires it.</summary>
    [JsonPropertyName("card_no")]
    public string? CardNo { get; set; }

    /// <summary>Street address.</summary>
    [JsonPropertyName("address")]
    public required string Address { get; set; }

    /// <summary>Sub-district (tambon).</summary>
    [JsonPropertyName("sub_district")]
    public required string SubDistrict { get; set; }

    /// <summary>District (amphoe).</summary>
    [JsonPropertyName("district")]
    public required string District { get; set; }

    /// <summary>Province.</summary>
    [JsonPropertyName("province")]
    public required string Province { get; set; }

    /// <summary>Postal code.</summary>
    [JsonPropertyName("zip_code")]
    public required string ZipCode { get; set; }
}

/// <summary><c>MerchantOrderShipmentCreateRequestRecipient</c>.</summary>
public sealed class MerchantOrderShipmentCreateRecipient
{
    /// <summary>Full name.</summary>
    [JsonPropertyName("fullname")]
    public required string Fullname { get; set; }

    /// <summary>E-mail address.</summary>
    [JsonPropertyName("email")]
    public string? Email { get; set; }

    /// <summary>Mobile phone number.</summary>
    [JsonPropertyName("mobile_phone")]
    public required string MobilePhone { get; set; }

    /// <summary>Street address.</summary>
    [JsonPropertyName("address")]
    public required string Address { get; set; }

    /// <summary>Sub-district (tambon).</summary>
    [JsonPropertyName("sub_district")]
    public required string SubDistrict { get; set; }

    /// <summary>District (amphoe).</summary>
    [JsonPropertyName("district")]
    public required string District { get; set; }

    /// <summary>Province.</summary>
    [JsonPropertyName("province")]
    public required string Province { get; set; }

    /// <summary>Postal code.</summary>
    [JsonPropertyName("zip_code")]
    public required string ZipCode { get; set; }
}

/// <summary><c>MerchantOrderShipmentCreateRequestPackage</c> — dimensions in cm (≥ 1), weight in grams.</summary>
public sealed class MerchantOrderShipmentCreatePackage
{
    /// <summary>Declared value to insure, in THB; <c>0</c> = no insurance.</summary>
    [JsonPropertyName("insurance_value")]
    public decimal InsuranceValue { get; set; }

    /// <summary>Free-text note.</summary>
    [JsonPropertyName("note")]
    public string? Note { get; set; }

    /// <summary>Content category. The server defaults to <see cref="FlashArticleCategory.Others"/> (99).</summary>
    [JsonPropertyName("type")]
    public FlashArticleCategory? Type { get; set; }

    /// <summary>Weight in grams.</summary>
    [JsonPropertyName("weight")]
    public required decimal Weight { get; set; }

    /// <summary>Width in cm.</summary>
    [JsonPropertyName("width")]
    public required decimal Width { get; set; }

    /// <summary>Height in cm.</summary>
    [JsonPropertyName("height")]
    public required decimal Height { get; set; }

    /// <summary>Length in cm.</summary>
    [JsonPropertyName("length")]
    public required decimal Length { get; set; }
}

/// <summary>
/// <c>MerchantOrderShipmentCreateRequestProductCod</c>. The COD amount collected from the recipient is the sum of
/// <c>price_per_item × amount</c> over all lines.
/// </summary>
public sealed class MerchantOrderShipmentCreateProductCod
{
    /// <summary>Product name.</summary>
    [JsonPropertyName("name")]
    public required string Name { get; set; }

    /// <summary>Quantity.</summary>
    [JsonPropertyName("amount")]
    public required int Amount { get; set; }

    /// <summary>Price per item in THB.</summary>
    [JsonPropertyName("price_per_item")]
    public required decimal PricePerItem { get; set; }
}

/// <summary>One parcel to create (<c>MerchantOrderShipmentCreateRequest</c>).</summary>
public sealed class MerchantOrderShipmentCreateRequest
{
    /// <summary>Courier and references.</summary>
    [JsonPropertyName("shipping")]
    public required MerchantOrderShipmentCreateShipping Shipping { get; set; }

    /// <summary>Sender.</summary>
    [JsonPropertyName("sender")]
    public required MerchantOrderShipmentCreateSender Sender { get; set; }

    /// <summary>Recipient.</summary>
    [JsonPropertyName("recipient")]
    public required MerchantOrderShipmentCreateRecipient Recipient { get; set; }

    /// <summary>Package.</summary>
    [JsonPropertyName("package")]
    public required MerchantOrderShipmentCreatePackage Package { get; set; }

    /// <summary>COD lines; leave empty for a non-COD parcel. Always sent.</summary>
    [JsonPropertyName("product_cods")]
    public List<MerchantOrderShipmentCreateProductCod> ProductCods { get; set; } = [];
}

// ---------------------------------------------------------------- calculate price

/// <summary>
/// <c>POST order-shipment/calculate-price</c> request. <c>r_*</c> = recipient area, <c>p_*</c> = parcel (weight in
/// grams, dimensions in cm, COD and insurance in THB).
/// </summary>
public sealed class MerchantOrderShipmentCalculatePriceRequest
{
    /// <summary>A <c>name</c> from <c>ShipmentProviders.AllAsync()</c>.</summary>
    [JsonPropertyName("shipment_name")]
    public required string ShipmentName { get; set; }

    /// <summary>Recipient sub-district.</summary>
    [JsonPropertyName("r_sub_district")]
    public required string RSubDistrict { get; set; }

    /// <summary>Recipient district.</summary>
    [JsonPropertyName("r_district")]
    public required string RDistrict { get; set; }

    /// <summary>Recipient province.</summary>
    [JsonPropertyName("r_province")]
    public required string RProvince { get; set; }

    /// <summary>Recipient postal code.</summary>
    [JsonPropertyName("r_zip_code")]
    public required string RZipCode { get; set; }

    /// <summary>Weight in grams.</summary>
    [JsonPropertyName("p_weight")]
    public required decimal PWeight { get; set; }

    /// <summary>Width in cm.</summary>
    [JsonPropertyName("p_width")]
    public required decimal PWidth { get; set; }

    /// <summary>Height in cm.</summary>
    [JsonPropertyName("p_height")]
    public required decimal PHeight { get; set; }

    /// <summary>Length in cm.</summary>
    [JsonPropertyName("p_length")]
    public required decimal PLength { get; set; }

    /// <summary>COD amount in THB.</summary>
    [JsonPropertyName("p_cod")]
    public decimal PCod { get; set; }

    /// <summary>Insured value in THB.</summary>
    [JsonPropertyName("p_insurance")]
    public decimal PInsurance { get; set; }
}

/// <summary>One price line (<c>PriceInfo</c>).</summary>
public sealed class PriceInfo
{
    /// <summary>A <see cref="PriceInfoDescription"/> value.</summary>
    [JsonPropertyName("description")]
    public string Description { get; set; } = "";

    /// <summary>Price charged to the store.</summary>
    [JsonPropertyName("price")]
    public decimal Price { get; set; }

    /// <summary>Cost.</summary>
    [JsonPropertyName("cost")]
    public decimal Cost { get; set; }

    /// <summary>Cashback cost.</summary>
    [JsonPropertyName("cashback_cost")]
    public decimal CashbackCost { get; set; }

    /// <summary>Whether cashback is rewarded.</summary>
    [JsonPropertyName("is_reward_cashback")]
    public bool IsRewardCashback { get; set; }

    /// <summary>Affiliate total (the wire spells it <c>total_affliliate</c>).</summary>
    [JsonPropertyName("total_affliliate")]
    public decimal TotalAffliliate { get; set; }
}

/// <summary>Which plan bracket the price came from (<c>OrderShipmentPlanDetail</c>).</summary>
public sealed class OrderShipmentPlanDetail
{
    /// <summary>A <see cref="PlanDetailRegion"/> value.</summary>
    [JsonPropertyName("region")]
    public string Region { get; set; } = "";

    /// <summary>A <see cref="PlanDetailType"/> value.</summary>
    [JsonPropertyName("type")]
    public string Type { get; set; } = "";

    /// <summary>Lower bound of the bracket.</summary>
    [JsonPropertyName("min_boundary")]
    public decimal MinBoundary { get; set; }

    /// <summary>Upper bound of the bracket.</summary>
    [JsonPropertyName("max_boundary")]
    public decimal MaxBoundary { get; set; }
}

/// <summary><c>MerchantOrderShipmentCalculatePriceResponse</c>.</summary>
public sealed class MerchantOrderShipmentCalculatePriceResponse
{
    /// <summary>Price lines.</summary>
    [JsonPropertyName("price_infos")]
    public List<PriceInfo> PriceInfos { get; set; } = [];

    /// <summary>Plan bracket.</summary>
    [JsonPropertyName("plan_detail")]
    public OrderShipmentPlanDetail PlanDetail { get; set; } = new();
}
