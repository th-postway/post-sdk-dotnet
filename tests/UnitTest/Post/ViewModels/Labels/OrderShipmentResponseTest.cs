using Postway.ViewModels.Labels;

namespace Postway.UnitTest.Post.ViewModels.Labels;

public class OrderShipmentResponseTest
{
    [Fact]
    public void DefaultValues_ShouldBeCorrect()
    {
        var resp = new OrderShipmentResponse();
        Assert.NotNull(resp.content);
        Assert.Empty(resp.content);
        Assert.Equal(0, resp.content_length);
        Assert.Equal("", resp.content_type);
        Assert.Equal("", resp.file_name);
    }

    [Fact]
    public void CanSetAndGetProperties()
    {
        var resp = new OrderShipmentResponse();
        var bytes = new byte[] { 1, 2, 3 };
        resp.content = bytes;
        resp.content_length = 3;
        resp.content_type = "application/pdf";
        resp.file_name = "test.pdf";

        Assert.Equal(bytes, resp.content);
        Assert.Equal(3, resp.content_length);
        Assert.Equal("application/pdf", resp.content_type);
        Assert.Equal("test.pdf", resp.file_name);
    }

    [Fact]
    public void PropertyTypes_ShouldBeCorrect()
    {
        var resp = new OrderShipmentResponse();
        Assert.IsType<byte[]>(resp.content);
        Assert.IsType<int>(resp.content_length);
        Assert.IsType<string>(resp.content_type);
        Assert.IsType<string>(resp.file_name);
    }
}