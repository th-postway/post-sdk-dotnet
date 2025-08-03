namespace Postway.IntegrationTest.Postway;

public class PostTest
{
    [Fact]
    public void Constructor_ShouldInitializeProperties()
    {
        var post = new Post("dummy_token");
        Assert.NotNull(post.Auth);
        Assert.NotNull(post.Label);
        Assert.NotNull(post.OrderShipment);
        Assert.NotNull(post.ShipmentProvider);
        Assert.NotNull(post.Thailand);
    }

    [Fact]
    public void Dispose_ShouldNotThrow()
    {
        var post = new Post("dummy_token");
        var ex = Record.Exception(() => post.Dispose());
        Assert.Null(ex);
    }
}
