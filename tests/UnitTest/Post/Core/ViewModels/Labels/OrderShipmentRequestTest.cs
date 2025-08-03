using Postway.Core.CONST;
using Postway.Core.ViewModels.Labels;

namespace Postway.UnitTest.Post.Core.ViewModels.Labels;

public class OrderShipmentRequestTest
{
    [Fact]
    public void DefaultValues_ShouldBeCorrect()
    {
        var req = new OrderShipmentRequest();
        Assert.Empty(req.tracking_nos);
        Assert.Equal(VALUE.LABEL_SIZE.SIX_BY_FOUR, req.label_size);
        Assert.Equal(VALUE.LABEL_ORIENTATION.PORTRAIT, req.label_orientation);
    }

    [Fact]
    public void CanSetTrackingNos()
    {
        var req = new OrderShipmentRequest();
        var list = new[] { "TN1", "TN2" };
        req.tracking_nos = list;
        Assert.Equal(list, req.tracking_nos);
    }

    [Fact]
    public void CanSetLabelSize()
    {
        var req = new OrderShipmentRequest();
        req.label_size = "CUSTOM_SIZE";
        Assert.Equal("CUSTOM_SIZE", req.label_size);
    }

    [Fact]
    public void CanSetLabelOrientation()
    {
        var req = new OrderShipmentRequest();
        req.label_orientation = "LANDSCAPE";
        Assert.Equal("LANDSCAPE", req.label_orientation);
    }
    
}