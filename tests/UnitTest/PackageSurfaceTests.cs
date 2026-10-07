using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using Postway.Models;

namespace Postway.UnitTest;

public class PackageSurfaceTests
{
    private static readonly string[] PublicTypes =
    [
        "Postway.AccessToken",
        "Postway.AccessTokenRefreshReason",
        "Postway.MerchantBaseUrls",
        "Postway.MerchantEnvironment",
        "Postway.Models.CalculatedRange",
        "Postway.Models.EnabledRange",
        "Postway.Models.FileHttpResponse",
        "Postway.Models.FilterRequest",
        "Postway.Models.FilterResponse`1",
        "Postway.Models.FlashArticleCategory",
        "Postway.Models.LabelOrientation",
        "Postway.Models.LabelSize",
        "Postway.Models.MerchantAccountSession",
        "Postway.Models.MerchantAccountStore",
        "Postway.Models.MerchantAccountUser",
        "Postway.Models.MerchantAuthAccountInfoResponse",
        "Postway.Models.MerchantLabelOrderShipmentsRequest",
        "Postway.Models.MerchantOrderShipmentCalculatePriceRequest",
        "Postway.Models.MerchantOrderShipmentCalculatePriceResponse",
        "Postway.Models.MerchantOrderShipmentCreatePackage",
        "Postway.Models.MerchantOrderShipmentCreateProductCod",
        "Postway.Models.MerchantOrderShipmentCreateRecipient",
        "Postway.Models.MerchantOrderShipmentCreateRequest",
        "Postway.Models.MerchantOrderShipmentCreateSender",
        "Postway.Models.MerchantOrderShipmentCreateShipping",
        "Postway.Models.MerchantOrderShipmentData",
        "Postway.Models.MerchantOrderShipmentDataPackage",
        "Postway.Models.MerchantOrderShipmentFilterRequest",
        "Postway.Models.MerchantOrderShipmentParty",
        "Postway.Models.MerchantShipmentProviderData",
        "Postway.Models.MerchantShipmentProviderFee",
        "Postway.Models.MerchantThailand",
        "Postway.Models.MerchantThailandFilterRequest",
        "Postway.Models.MinMax",
        "Postway.Models.OrderShipmentChannel",
        "Postway.Models.OrderShipmentPlanDetail",
        "Postway.Models.OrderShipmentStatus",
        "Postway.Models.OrderStatus",
        "Postway.Models.PlanDetailRegion",
        "Postway.Models.PlanDetailType",
        "Postway.Models.PriceInfo",
        "Postway.Models.PriceInfoDescription",
        "Postway.Models.PublicReceiptPriceInfo",
        "Postway.Models.PublicReceiptResponse",
        "Postway.Models.PublicReceiptShipment",
        "Postway.Models.ReceiptSize",
        "Postway.Models.StoreBillingType",
        "Postway.Models.StoreCodType",
        "Postway.Models.UserRole",
        "Postway.PostwayApiException",
        "Postway.PostwayBusinessException",
        "Postway.PostwayConfigException",
        "Postway.PostwayException",
        "Postway.PostwayMerchantClient",
        "Postway.PostwayMerchantClientOptions",
        "Postway.PostwayRequestException",
        "Postway.PostwaySdk",
        "Postway.RequestOptions",
        "Postway.Resources.AuthResource",
        "Postway.Resources.HealthResource",
        "Postway.Resources.LabelsResource",
        "Postway.Resources.OrderShipmentsResource",
        "Postway.Resources.ReceiptsResource",
        "Postway.Resources.ShipmentProvidersResource",
        "Postway.Resources.ThailandResource",
    ];

    [Fact]
    public void SdkVersionMatchesThePackageVersion()
    {
        var assembly = typeof(PostwayMerchantClient).Assembly;
        var informational = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()!.InformationalVersion;
        Assert.Equal(PostwaySdk.Version, informational);
        Assert.Equal(new Version(PostwaySdk.Version + ".0"), assembly.GetName().Version);
    }

    [Fact]
    public void ExportsTheClientErrorsResourcesAndModels()
    {
        var actual = typeof(PostwayMerchantClient).Assembly
            .GetExportedTypes()
            .Where(type => !type.IsNested)
            .Select(type => type.FullName!)
            .Order(StringComparer.Ordinal);
        Assert.Equal(PublicTypes.Order(StringComparer.Ordinal), actual);
    }

    [Fact]
    public void HasNoRuntimeDependenciesBeyondTheFramework()
    {
        var references = typeof(PostwayMerchantClient).Assembly.GetReferencedAssemblies().Select(name => name.Name!);
        Assert.All(references, name => Assert.True(name.StartsWith("System", StringComparison.Ordinal) || name == "netstandard", name));
    }

    private static readonly JsonSerializerOptions Wire = new()
    {
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
    };

    private static string[] Keys(object model) =>
        [.. JsonNode.Parse(JsonSerializer.Serialize(model, model.GetType(), Wire))!.AsObject().Select(pair => pair.Key).Order(StringComparer.Ordinal)];

    [Fact]
    public void RequestModelsUseTheWireNames()
    {
        Assert.Equal(
            ["filter", "limit", "page"],
            Keys(new MerchantOrderShipmentFilterRequest { Filter = "x", Page = 1, Limit = 1 }));
        Assert.Equal(
            ["district", "filter", "limit", "page", "province", "shipment_provider_names", "sub_district", "zip_code"],
            Keys(new MerchantThailandFilterRequest
            {
                Filter = "x",
                Page = 1,
                Limit = 1,
                ShipmentProviderNames = [],
                SubDistrict = "a",
                District = "b",
                Province = "c",
                ZipCode = "d",
            }));
        Assert.Equal(
            ["label_orientation", "label_size", "tracking_nos"],
            Keys(new MerchantLabelOrderShipmentsRequest { TrackingNos = [], LabelSize = LabelSize.SizeA4, LabelOrientation = LabelOrientation.Landscape }));
        Assert.Equal(
            ["p_cod", "p_height", "p_insurance", "p_length", "p_weight", "p_width", "r_district", "r_province", "r_sub_district", "r_zip_code", "shipment_name"],
            Keys(new MerchantOrderShipmentCalculatePriceRequest
            {
                ShipmentName = "Flash",
                RSubDistrict = "a",
                RDistrict = "b",
                RProvince = "c",
                RZipCode = "d",
                PWeight = 1,
                PWidth = 1,
                PHeight = 1,
                PLength = 1,
            }));
        Assert.Equal(
            ["my_tracking_no", "shipment_provider_name", "shipment_provider_tracking_no"],
            Keys(new MerchantOrderShipmentCreateShipping { ShipmentProviderName = "Flash", MyTrackingNo = "a", ShipmentProviderTrackingNo = "b" }));
        Assert.Equal(
            ["address", "card_no", "district", "email", "fullname", "mobile_phone", "province", "sub_district", "zip_code"],
            Keys(new MerchantOrderShipmentCreateSender
            {
                Fullname = "a",
                Email = "b",
                MobilePhone = "c",
                CardNo = "d",
                Address = "e",
                SubDistrict = "f",
                District = "g",
                Province = "h",
                ZipCode = "i",
            }));
        Assert.Equal(
            ["address", "district", "email", "fullname", "mobile_phone", "province", "sub_district", "zip_code"],
            Keys(new MerchantOrderShipmentCreateRecipient
            {
                Fullname = "a",
                Email = "b",
                MobilePhone = "c",
                Address = "e",
                SubDistrict = "f",
                District = "g",
                Province = "h",
                ZipCode = "i",
            }));
        Assert.Equal(
            ["height", "insurance_value", "length", "note", "type", "weight", "width"],
            Keys(new MerchantOrderShipmentCreatePackage { Note = "n", Type = FlashArticleCategory.Others, Weight = 1, Width = 1, Height = 1, Length = 1 }));
        Assert.Equal(
            ["amount", "name", "price_per_item"],
            Keys(new MerchantOrderShipmentCreateProductCod { Name = "a", Amount = 1, PricePerItem = 1 }));
    }

    [Fact]
    public void OptionalRequestFieldsAreLeftOutWhenUnset()
    {
        Assert.Equal(["shipment_provider_name"], Keys(new MerchantOrderShipmentCreateShipping { ShipmentProviderName = "Flash" }));
        Assert.Equal(
            ["height", "insurance_value", "length", "weight", "width"],
            Keys(new MerchantOrderShipmentCreatePackage { Weight = 1, Width = 1, Height = 1, Length = 1 }));
    }

    [Fact]
    public void FilesDecodeFromBase64()
    {
        var file = new FileHttpResponse { Content = Convert.ToBase64String([1, 2, 3]) };
        Assert.Equal([1, 2, 3], file.DecodeContent());
    }
}
