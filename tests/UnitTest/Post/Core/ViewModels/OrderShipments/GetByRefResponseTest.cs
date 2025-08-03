using Postway.Core.ViewModels.OrderShipments;

namespace Postway.UnitTest.Post.Core.ViewModels.OrderShipments;

public class GetByRefResponseTest
{
    [Fact]
    public void DefaultConstructor_InitializesPropertiesCorrectly()
    {
        var response = new GetByRefResponse();
        Assert.Equal("", response.channel);
        Assert.NotNull(response.sender);
        Assert.NotNull(response.recipient);
        Assert.NotNull(response.package);
        Assert.Equal("", response.order_shipment_status);
        Assert.Equal("", response.status);
        Assert.Null(response.completed_at);
        Assert.Null(response.in_transit_at);
        Assert.IsType<DateTime>(response.created_at);
        Assert.IsType<DateTime>(response.updated_at);
    }

    [Fact]
    public void Sender_DefaultValues_AreCorrect()
    {
        var sender = new Postway.Core.Models.OrderShipment.Sender();
        Assert.Equal("", sender.fullname);
        Assert.Equal("", sender.mobile_phone);
        Assert.Equal("", sender.address);
        Assert.Equal("", sender.sub_district);
        Assert.Equal("", sender.district);
        Assert.Equal("", sender.province);
        Assert.Equal("", sender.zip_code);
    }

    [Fact]
    public void Recipient_DefaultValues_AreCorrect()
    {
        var recipient = new Postway.Core.Models.OrderShipment.Recipient();
        Assert.Equal("", recipient.fullname);
        Assert.Equal("", recipient.mobile_phone);
        Assert.Equal("", recipient.address);
        Assert.Equal("", recipient.sub_district);
        Assert.Equal("", recipient.district);
        Assert.Equal("", recipient.province);
        Assert.Equal("", recipient.zip_code);
    }

    [Fact]
    public void Package_DefaultValues_AreCorrect()
    {
        var package = new Postway.Core.Models.OrderShipment.Package();
        Assert.Equal("", package.my_tracking_no);
        Assert.Equal("", package.tracking_no);
        Assert.Equal("", package.ref1);
        Assert.Equal("", package.ref2);
        Assert.Equal("", package.ref3);
        Assert.Equal(0, package.weight);
        Assert.Equal(0, package.width);
        Assert.Equal(0, package.length);
        Assert.Equal(0, package.height);
    }

    [Fact]
    public void CanSetProperties()
    {
        var response = new GetByRefResponse
        {
            channel = "testChannel",
            order_shipment_status = "delivered",
            status = "success",
            completed_at = DateTime.Now,
            in_transit_at = DateTime.Now.AddHours(-1),
            created_at = DateTime.Today,
            updated_at = DateTime.Today.AddDays(1),
            sender = new Postway.Core.Models.OrderShipment.Sender { fullname = "Sender Name" },
            recipient = new Postway.Core.Models.OrderShipment.Recipient { fullname = "Recipient Name" },
            package = new Postway.Core.Models.OrderShipment.Package { my_tracking_no = "123" }
        };
        Assert.Equal("testChannel", response.channel);
        Assert.Equal("delivered", response.order_shipment_status);
        Assert.Equal("success", response.status);
        Assert.Equal("Sender Name", response.sender.fullname);
        Assert.Equal("Recipient Name", response.recipient.fullname);
        Assert.Equal("123", response.package.my_tracking_no);
    }
}