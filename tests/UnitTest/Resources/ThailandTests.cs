using Postway.Models;
using static Postway.UnitTest.Support.Stub;

namespace Postway.UnitTest.Resources;

public class ThailandTests
{
    private const string Page = """
        {"count":1,"limit":10,"page":1,"page_count":1,"data":[{"shipment_provider_names":["Flash"],"sub_district":"สุเทพ",
         "district":"เมืองเชียงใหม่","province":"เชียงใหม่","zipcode":"50200","is_island":false,"is_tourist":true,"is_remote_area":false}]}
        """;

    [Fact]
    public async Task FilterPostsAndAlwaysSendsShipmentProviderNames()
    {
        var (client, handler) = Setup(Json(Page, 201), Json(Page, 201));
        var page = await client.Thailand.FilterAsync(new MerchantThailandFilterRequest { Page = 1, Limit = 10, ZipCode = "50200" });
        await client.Thailand.FilterAsync(new MerchantThailandFilterRequest { Page = 1, Limit = 10, ShipmentProviderNames = ["Flash"] });

        var area = Assert.Single(page.Data);
        Assert.Equal("50200", area.Zipcode);
        Assert.True(area.IsTourist);
        Assert.Equal(["Flash"], area.ShipmentProviderNames);

        var first = handler.Calls[0];
        Assert.Equal("POST", first.Method);
        Assert.Equal($"{BaseUrl}/thailand/filter", first.Url);
        Assert.Equal(Auth, first.Headers["Authorization"]);
        Assert.Equal("application/json", first.Headers["Content-Type"]);
        JsonEqual("""{"page":1,"limit":10,"zip_code":"50200","shipment_provider_names":[]}""", first.Body);
        JsonEqual("""{"page":1,"limit":10,"shipment_provider_names":["Flash"]}""", handler.Calls[1].Body);
    }

    [Fact]
    public async Task FilterDoesNotMutateTheCallersRequest()
    {
        var (client, _) = Setup(Json(Page));
        var request = new MerchantThailandFilterRequest { Page = 1, Limit = 10 };
        await client.Thailand.FilterAsync(request);
        Assert.Null(request.ShipmentProviderNames);
    }
}
