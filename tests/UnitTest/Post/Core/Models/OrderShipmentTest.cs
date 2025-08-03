using System;
using Xunit;
using Postway.Core.Models;

namespace Postway.UnitTest.Post.Core.Models;

public class OrderShipmentTest
{
    [Fact]
    public void DefaultConstructor_InitializesPropertiesCorrectly()
    {
        var shipment = new OrderShipment();
        Assert.Equal("", shipment.channel);
        Assert.NotNull(shipment.sender);
        Assert.NotNull(shipment.recipient);
        Assert.NotNull(shipment.package);
        Assert.Equal("", shipment.order_shipment_status);
        Assert.Equal("", shipment.status);
        Assert.Null(shipment.completed_at);
        Assert.Null(shipment.in_transit_at);
        Assert.True(shipment.created_at <= DateTime.Now);
        Assert.True(shipment.updated_at <= DateTime.Now);
    }

    [Fact]
    public void Sender_DefaultValues_AreCorrect()
    {
        var sender = new OrderShipment.Sender();
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
        var recipient = new OrderShipment.Recipient();
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
        var package = new OrderShipment.Package();
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
        var shipment = new OrderShipment
        {
            channel = "test_channel",
            order_shipment_status = "shipped",
            status = "completed",
            completed_at = DateTime.Now,
            in_transit_at = DateTime.Now.AddHours(-1),
            created_at = DateTime.Now.AddDays(-1),
            updated_at = DateTime.Now
        };
        shipment.sender.fullname = "Sender Name";
        shipment.recipient.fullname = "Recipient Name";
        shipment.package.tracking_no = "TRACK123";

        Assert.Equal("test_channel", shipment.channel);
        Assert.Equal("shipped", shipment.order_shipment_status);
        Assert.Equal("completed", shipment.status);
        Assert.NotNull(shipment.completed_at);
        Assert.NotNull(shipment.in_transit_at);
        Assert.True(shipment.created_at < DateTime.Now);
        Assert.True(shipment.updated_at <= DateTime.Now);
        Assert.Equal("Sender Name", shipment.sender.fullname);
        Assert.Equal("Recipient Name", shipment.recipient.fullname);
        Assert.Equal("TRACK123", shipment.package.tracking_no);
    }
}
