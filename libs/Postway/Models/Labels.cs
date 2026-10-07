using System.Text.Json.Serialization;

namespace Postway.Models;

/// <summary><c>POST label/order/shipments</c> request (<c>MerchantLabelOrderShipmentsRequest</c>).</summary>
public sealed class MerchantLabelOrderShipmentsRequest
{
    /// <summary>Each entry may be a <c>tracking_no</c>, <c>my_tracking_no</c> or <c>ref1..3</c>.</summary>
    [JsonPropertyName("tracking_nos")]
    public required List<string> TrackingNos { get; set; }

    /// <summary>A <see cref="Models.LabelSize"/> value.</summary>
    [JsonPropertyName("label_size")]
    public required string LabelSize { get; set; }

    /// <summary>A <see cref="Models.LabelOrientation"/> value.</summary>
    [JsonPropertyName("label_orientation")]
    public required string LabelOrientation { get; set; }
}
