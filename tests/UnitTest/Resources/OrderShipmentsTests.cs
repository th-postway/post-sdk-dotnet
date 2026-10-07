using System.Text.Json.Nodes;
using Postway.Models;
using static Postway.UnitTest.Support.Stub;

namespace Postway.UnitTest.Resources;

public class OrderShipmentsTests
{
    private const string Shipment = """
        {
          "channel": "API",
          "sender": { "fullname": "ร้านตัวอย่าง", "mobile_phone": "0811111111", "address": "1 ถนนสีลม",
                      "sub_district": "สีลม", "district": "บางรัก", "province": "กรุงเทพมหานคร", "zip_code": "10500" },
          "recipient": { "fullname": "ลูกค้า", "mobile_phone": "0822222222", "address": "2 ถนนนิมมานเหมินท์",
                         "sub_district": "สุเทพ", "district": "เมืองเชียงใหม่", "province": "เชียงใหม่", "zip_code": "50200" },
          "package": { "width": 10, "length": 20, "height": 5, "weight": 500, "my_tracking_no": "SHOP-1",
                       "tracking_no": "TH0001", "ref1": "R1", "ref2": "", "ref3": "" },
          "status": "pending",
          "order_shipment_status": "Prepared",
          "created_at": "2026-10-06T03:00:00.000Z",
          "updated_at": null,
          "in_transit_at": null,
          "completed_at": null
        }
        """;

    private static readonly string CreateRequestJson = """
        {
          "shipping": { "shipment_provider_name": "Flash", "my_tracking_no": "SHOP-1" },
          "sender": { "fullname": "ร้านตัวอย่าง", "mobile_phone": "0811111111", "address": "1 ถนนสีลม",
                      "sub_district": "สีลม", "district": "บางรัก", "province": "กรุงเทพมหานคร", "zip_code": "10500" },
          "recipient": { "fullname": "ลูกค้า", "mobile_phone": "0822222222", "address": "2 ถนนนิมมานเหมินท์",
                         "sub_district": "สุเทพ", "district": "เมืองเชียงใหม่", "province": "เชียงใหม่", "zip_code": "50200" },
          "package": { "insurance_value": 0, "type": 4, "weight": 500, "width": 10, "height": 5, "length": 20 },
          "product_cods": [{ "name": "เสื้อ", "amount": 2, "price_per_item": 150 }]
        }
        """;

    private static MerchantOrderShipmentCreateRequest CreateRequest() => new()
    {
        Shipping = new() { ShipmentProviderName = "Flash", MyTrackingNo = "SHOP-1" },
        Sender = new()
        {
            Fullname = "ร้านตัวอย่าง",
            MobilePhone = "0811111111",
            Address = "1 ถนนสีลม",
            SubDistrict = "สีลม",
            District = "บางรัก",
            Province = "กรุงเทพมหานคร",
            ZipCode = "10500",
        },
        Recipient = new()
        {
            Fullname = "ลูกค้า",
            MobilePhone = "0822222222",
            Address = "2 ถนนนิมมานเหมินท์",
            SubDistrict = "สุเทพ",
            District = "เมืองเชียงใหม่",
            Province = "เชียงใหม่",
            ZipCode = "50200",
        },
        Package = new() { InsuranceValue = 0, Type = FlashArticleCategory.Clothes, Weight = 500, Width = 10, Height = 5, Length = 20 },
        ProductCods = [new() { Name = "เสื้อ", Amount = 2, PricePerItem = 150 }],
    };

    private static HttpResponseMessage Ok(JsonNode? data) => Envelope(201, 200, true, JsonValue.Create("ok"), data);

    [Fact]
    public async Task GetByTrackingNoSendsGetWithTheTrackingNumber()
    {
        var (client, handler) = Setup(Json(Shipment));
        var parcel = await client.OrderShipments.GetByTrackingNoAsync("TH0001");
        Assert.NotNull(parcel);
        Assert.Equal("TH0001", parcel.Package.TrackingNo);
        Assert.Equal(OrderShipmentStatus.Prepared, parcel.OrderShipmentStatus);
        Assert.Equal(OrderShipmentChannel.API, parcel.Channel);
        Assert.Equal("สีลม", parcel.Sender.SubDistrict);
        Assert.Equal(new DateTimeOffset(2026, 10, 6, 3, 0, 0, TimeSpan.Zero), parcel.CreatedAt);
        Assert.Null(parcel.UpdatedAt);
        var call = Only(handler);
        Assert.Equal("GET", call.Method);
        Assert.Equal($"{BaseUrl}/order-shipment/get-by-tracking-no/TH0001", call.Url);
        Assert.Equal(Auth, call.Headers["Authorization"]);
        Assert.Null(call.Body);
    }

    [Fact]
    public async Task GetByTrackingNoReturnsNullOnAnEmptyBody()
    {
        var (client, _) = Setup(Empty());
        Assert.Null(await client.OrderShipments.GetByTrackingNoAsync("NOPE"));
    }

    [Fact]
    public async Task GetByTrackingNoReturnsNullOnJsonNull()
    {
        var (client, _) = Setup(Json("null"));
        Assert.Null(await client.OrderShipments.GetByTrackingNoAsync("NOPE"));
    }

    [Fact]
    public async Task GetByRefSendsGetAndReturnsNullWhenNotFound()
    {
        var (client, handler) = Setup(Json(Shipment), Empty());
        Assert.Equal("R1", (await client.OrderShipments.GetByRefAsync("R1"))!.Package.Ref1);
        Assert.Null(await client.OrderShipments.GetByRefAsync("R404"));
        Assert.Equal(
            [$"{BaseUrl}/order-shipment/get-by-ref/R1", $"{BaseUrl}/order-shipment/get-by-ref/R404"],
            handler.Calls.Select(c => c.Url));
        Assert.All(handler.Calls, c => Assert.Equal("GET", c.Method));
        Assert.All(handler.Calls, c => Assert.Equal(Auth, c.Headers["Authorization"]));
    }

    [Fact]
    public async Task FilterPostsThePagingBody()
    {
        var (client, handler) = Setup(Json($$"""{"count":1,"limit":20,"page":1,"page_count":1,"data":[{{Shipment}}]}""", 201));
        var page = await client.OrderShipments.FilterAsync(new MerchantOrderShipmentFilterRequest { Filter = "TH00", Page = 1, Limit = 20 });
        Assert.Equal(1, page.Count);
        Assert.Equal(1, page.PageCount);
        Assert.Equal("TH0001", Assert.Single(page.Data).Package.TrackingNo);
        var call = Only(handler);
        Assert.Equal("POST", call.Method);
        Assert.Equal($"{BaseUrl}/order-shipment/filter", call.Url);
        JsonEqual("""{"filter":"TH00","page":1,"limit":20}""", call.Body);
        Assert.Equal("application/json", call.Headers["Content-Type"]);
        Assert.Equal(Auth, call.Headers["Authorization"]);
    }

    [Fact]
    public async Task FilterLeavesOutAnUnsetFilter()
    {
        var (client, handler) = Setup(Json("""{"count":0,"limit":5,"page":1,"page_count":0,"data":[]}"""));
        await client.OrderShipments.FilterAsync(new MerchantOrderShipmentFilterRequest { Page = 1, Limit = 5 });
        JsonEqual("""{"page":1,"limit":5}""", Only(handler).Body);
    }

    [Fact]
    public async Task CreatePostsAnArrayBodyAndUnwrapsData()
    {
        var (client, handler) = Setup(Ok(new JsonArray(JsonNode.Parse(Shipment))));
        var created = await client.OrderShipments.CreateAsync([CreateRequest()]);
        Assert.Equal("TH0001", Assert.Single(created).Package.TrackingNo);
        var call = Only(handler);
        Assert.Equal("POST", call.Method);
        Assert.Equal($"{BaseUrl}/order-shipment/create", call.Url);
        Assert.Equal(Auth, call.Headers["Authorization"]);
        Assert.Equal("application/json", call.Headers["Content-Type"]);
        JsonEqual($"[{CreateRequestJson}]", call.Body);
    }

    [Fact]
    public async Task CreateWrapsASingleRequestIntoTheArrayTheServerExpects()
    {
        var (client, handler) = Setup(Ok(new JsonArray(JsonNode.Parse(Shipment))));
        await client.OrderShipments.CreateAsync(CreateRequest());
        JsonEqual($"[{CreateRequestJson}]", Only(handler).Body);
    }

    [Fact]
    public async Task CreateRaisesPostwayBusinessExceptionOnA201Failure()
    {
        var (client, _) = Setup(Envelope(201, 400, false, JsonValue.Create("ขนส่งปฏิเสธพัสดุ"), null));
        var error = await Assert.ThrowsAsync<PostwayBusinessException>(() => client.OrderShipments.CreateAsync(CreateRequest()));
        Assert.Equal(201, error.Status);
        Assert.Equal(["ขนส่งปฏิเสธพัสดุ"], error.Messages);
    }

    [Fact]
    public async Task CalculatePricePostsTheRequest()
    {
        const string quote = """
            {
              "price_infos": [{ "description": "ค่าขนส่ง", "price": 35, "cost": 25, "cashback_cost": 0,
                                "is_reward_cashback": false, "total_affliliate": 0 }],
              "plan_detail": { "region": "UPC", "type": "weight", "min_boundary": 0, "max_boundary": 1000 }
            }
            """;
        var (client, handler) = Setup(Json(quote, 201));
        var result = await client.OrderShipments.CalculatePriceAsync(new MerchantOrderShipmentCalculatePriceRequest
        {
            ShipmentName = "Flash",
            RSubDistrict = "สุเทพ",
            RDistrict = "เมืองเชียงใหม่",
            RProvince = "เชียงใหม่",
            RZipCode = "50200",
            PWeight = 500,
            PWidth = 10,
            PHeight = 5,
            PLength = 20,
            PCod = 300,
            PInsurance = 0,
        });
        var line = Assert.Single(result.PriceInfos);
        Assert.Equal(PriceInfoDescription.Shipment, line.Description);
        Assert.Equal(35m, line.Price);
        Assert.Equal(PlanDetailRegion.UPC, result.PlanDetail.Region);
        Assert.Equal(PlanDetailType.Weight, result.PlanDetail.Type);
        Assert.Equal(1000m, result.PlanDetail.MaxBoundary);
        var call = Only(handler);
        Assert.Equal("POST", call.Method);
        Assert.Equal($"{BaseUrl}/order-shipment/calculate-price", call.Url);
        Assert.Equal(Auth, call.Headers["Authorization"]);
        JsonEqual("""
            {
              "shipment_name": "Flash", "r_sub_district": "สุเทพ", "r_district": "เมืองเชียงใหม่",
              "r_province": "เชียงใหม่", "r_zip_code": "50200", "p_weight": 500, "p_width": 10,
              "p_height": 5, "p_length": 20, "p_cod": 300, "p_insurance": 0
            }
            """, call.Body);
    }

    [Fact]
    public async Task CancelPostsTheTrackingNumber()
    {
        var (client, handler) = Setup(Ok(null));
        await client.OrderShipments.CancelAsync("TH0001");
        var call = Only(handler);
        Assert.Equal("POST", call.Method);
        Assert.Equal($"{BaseUrl}/order-shipment/cancel", call.Url);
        Assert.Equal(Auth, call.Headers["Authorization"]);
        Assert.Equal("application/json", call.Headers["Content-Type"]);
        JsonEqual("""{"tracking_no":"TH0001"}""", call.Body);
    }
}
