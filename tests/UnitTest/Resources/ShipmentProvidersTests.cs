using static Postway.UnitTest.Support.Stub;

namespace Postway.UnitTest.Resources;

public class ShipmentProvidersTests
{
    [Fact]
    public async Task AllGetsTheCouriers()
    {
        var (client, handler) = Setup(Json("""
            [{"name":"Flash","display_name":"Flash Express","width":{"min":1,"max":100},
              "weight":{"min":1,"max":50000,"is_calculate":true},"cod":{"min":0,"max":50000,"enable":true},
              "fee":{"cod_postway_to_customer":1.5,"cod_customer_to_mass":0}}]
            """));
        var providers = await client.ShipmentProviders.AllAsync();
        var flash = Assert.Single(providers);
        Assert.Equal("Flash", flash.Name);
        Assert.Equal("Flash Express", flash.DisplayName);
        Assert.Equal(100m, flash.Width.Max);
        Assert.True(flash.Weight.IsCalculate);
        Assert.True(flash.Cod.Enable);
        Assert.Equal(1.5m, flash.Fee.CodPostwayToCustomer);
        var call = Only(handler);
        Assert.Equal("GET", call.Method);
        Assert.Equal($"{BaseUrl}/shipment-provider/all", call.Url);
        Assert.Equal(Auth, call.Headers["Authorization"]);
        Assert.Null(call.Body);
    }
}
