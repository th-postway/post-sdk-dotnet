using Postway.Models;
using static Postway.UnitTest.Support.Stub;

namespace Postway.UnitTest.Resources;

public class AuthTests
{
    [Fact]
    public async Task AccountInfoPostsWithTheBearerTokenAndNoBody()
    {
        var (client, handler) = Setup(Json("""
            {
              "store": { "code": "S1", "name": "Shop", "store_billing_type": "Top Up", "store_cod_type": "receipt", "zip_code": "10500" },
              "user": { "username": "owner", "first_name": "สมชาย", "role": "Admin" },
              "session": { "expired": "2027-01-01T00:00:00.000Z" }
            }
            """, 201));
        var info = await client.Auth.AccountInfoAsync();
        Assert.Equal("S1", info.Store.Code);
        Assert.Equal(StoreBillingType.TopUp, info.Store.StoreBillingType);
        Assert.Equal(StoreCodType.Receipt, info.Store.StoreCodType);
        Assert.Equal("owner", info.User.Username);
        Assert.Equal("สมชาย", info.User.FirstName);
        Assert.Equal(UserRole.Admin, info.User.Role);
        Assert.Equal(new DateTimeOffset(2027, 1, 1, 0, 0, 0, TimeSpan.Zero), info.Session.Expired);
        var call = Only(handler);
        Assert.Equal("POST", call.Method);
        Assert.Equal($"{BaseUrl}/auth/account/info", call.Url);
        Assert.Equal(Auth, call.Headers["Authorization"]);
        Assert.Null(call.Body);
    }
}
