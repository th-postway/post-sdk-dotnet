using Postway.ViewModels.Auths;

namespace Postway.UnitTest.Post.ViewModels.Auths;

public class AccountInfoResponseTest
{
    [Fact]
    public void AccountInfoResponse_DefaultValues_ShouldBeInitialized()
    {
        var response = new AccountInfoResponse();

        Assert.NotNull(response.user);
        Assert.NotNull(response.store);
        Assert.NotNull(response.session);

        // User defaults
        Assert.Equal("", response.user.username);
        Assert.Equal("", response.user.email);
        Assert.Equal("", response.user.first_name);
        Assert.Equal("", response.user.last_name);
        Assert.Equal("", response.user.mobile_phone);
        Assert.Equal("", response.user.role);

        // Store defaults
        Assert.Equal("", response.store.code);
        Assert.Equal("", response.store.name);
        Assert.Equal("", response.store.mobile_phone);
        Assert.Equal("", response.store.store_billing_type);
        Assert.Equal("", response.store.store_cod_type);
        Assert.Equal("", response.store.address);
        Assert.Equal("", response.store.sub_district);
        Assert.Equal("", response.store.district);
        Assert.Equal("", response.store.province);
        Assert.Equal("", response.store.zip_code);

        // Session default
        Assert.IsType<DateTime>(response.session.expired);
    }

    [Fact]
    public void AccountInfoResponse_GetterSetter_ShouldWorkCorrectly()
    {
        var response = new AccountInfoResponse();

        // Set User properties
        response.user.username = "testuser";
        response.user.email = "test@example.com";
        response.user.first_name = "John";
        response.user.last_name = "Doe";
        response.user.mobile_phone = "1234567890";
        response.user.role = "admin";

        Assert.Equal("testuser", response.user.username);
        Assert.Equal("test@example.com", response.user.email);
        Assert.Equal("John", response.user.first_name);
        Assert.Equal("Doe", response.user.last_name);
        Assert.Equal("1234567890", response.user.mobile_phone);
        Assert.Equal("admin", response.user.role);

        // Set Store properties
        response.store.code = "S001";
        response.store.name = "Main Store";
        response.store.mobile_phone = "0987654321";
        response.store.store_billing_type = "prepaid";
        response.store.store_cod_type = "cod";
        response.store.address = "123 Main St";
        response.store.sub_district = "Central";
        response.store.district = "Metro";
        response.store.province = "Bangkok";
        response.store.zip_code = "10100";

        Assert.Equal("S001", response.store.code);
        Assert.Equal("Main Store", response.store.name);
        Assert.Equal("0987654321", response.store.mobile_phone);
        Assert.Equal("prepaid", response.store.store_billing_type);
        Assert.Equal("cod", response.store.store_cod_type);
        Assert.Equal("123 Main St", response.store.address);
        Assert.Equal("Central", response.store.sub_district);
        Assert.Equal("Metro", response.store.district);
        Assert.Equal("Bangkok", response.store.province);
        Assert.Equal("10100", response.store.zip_code);

        // Set Session property
        var dt = new DateTime(2030, 1, 1);
        response.session.expired = dt;
        Assert.Equal(dt, response.session.expired);
    }
}