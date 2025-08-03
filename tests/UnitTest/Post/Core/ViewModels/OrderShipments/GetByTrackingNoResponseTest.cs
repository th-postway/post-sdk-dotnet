using Postway.Core.ViewModels.OrderShipments;

namespace Postway.UnitTest.Post.Core.ViewModels.OrderShipments;

public class GetByTrackingNoResponseTest
{
    [Fact]
    public void CanInstantiate_GetByTrackingNoResponse()
    {
        var response = new GetByTrackingNoResponse();
        Assert.NotNull(response);
    }

    [Fact]
    public void DefaultPropertyValues_AreCorrect()
    {
        var response = new GetByTrackingNoResponse();
        Assert.Equal("", response.channel);
        Assert.Equal("", response.order_shipment_status);
        Assert.Equal("", response.status);
        Assert.NotNull(response.sender);
        Assert.NotNull(response.recipient);
        Assert.NotNull(response.package);
        Assert.Null(response.completed_at);
        Assert.Null(response.in_transit_at);
        Assert.True(response.created_at <= DateTime.Now);
        Assert.True(response.updated_at <= DateTime.Now);
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
    public void CanSetAndGet_Properties()
    {
        var response = new GetByTrackingNoResponse
        {
            channel = "test_channel",
            order_shipment_status = "shipped",
            status = "completed",
            completed_at = DateTime.Now,
            in_transit_at = DateTime.Now.AddHours(-1),
            created_at = DateTime.Now.AddDays(-1),
            updated_at = DateTime.Now
        };
        response.sender.fullname = "Sender Name";
        response.recipient.fullname = "Recipient Name";
        response.package.tracking_no = "TRACK123";

        Assert.Equal("test_channel", response.channel);
        Assert.Equal("shipped", response.order_shipment_status);
        Assert.Equal("completed", response.status);
        Assert.NotNull(response.completed_at);
        Assert.NotNull(response.in_transit_at);
        Assert.Equal("Sender Name", response.sender.fullname);
        Assert.Equal("Recipient Name", response.recipient.fullname);
        Assert.Equal("TRACK123", response.package.tracking_no);
    }
}