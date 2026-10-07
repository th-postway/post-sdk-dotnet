using System.Text;
using Postway.Models;
using static Postway.UnitTest.Support.Stub;

namespace Postway.UnitTest.Resources;

public class LabelsTests
{
    private static readonly string File =
        $$"""{"file_name":"label.pdf","content":"{{Convert.ToBase64String(Encoding.ASCII.GetBytes("%PDF-1.7"))}}","content_type":"application/pdf","content_length":8}""";

    [Fact]
    public async Task OrderShipmentsPostsTheRequestAndTheFileDecodes()
    {
        var (client, handler) = Setup(Json(File, 201));
        var result = await client.Labels.OrderShipmentsAsync(new MerchantLabelOrderShipmentsRequest
        {
            TrackingNos = ["TH0001", "SHOP-2"],
            LabelSize = LabelSize.Size4x6,
            LabelOrientation = LabelOrientation.Portrait,
        });
        var call = Only(handler);
        Assert.Equal("POST", call.Method);
        Assert.Equal($"{BaseUrl}/label/order/shipments", call.Url);
        Assert.Equal(Auth, call.Headers["Authorization"]);
        Assert.Equal("application/json", call.Headers["Content-Type"]);
        JsonEqual("""{"tracking_nos":["TH0001","SHOP-2"],"label_size":"4x6","label_orientation":"Portrait"}""", call.Body);
        Assert.Equal("label.pdf", result.FileName);
        Assert.Equal("application/pdf", result.ContentType);
        Assert.Equal(8, result.ContentLength);
        Assert.Equal("%PDF-1.7", Encoding.ASCII.GetString(result.DecodeContent()));
    }

    [Fact]
    public async Task ReceiptGetsByReceiptNumberWithAnOptionalReceiptSize()
    {
        var (client, handler) = Setup(Json(File), Json(File));
        await client.Labels.ReceiptAsync("RC-001", ReceiptSize.R80mm);
        await client.Labels.ReceiptAsync("RC-002");
        Assert.Equal(
            [$"{BaseUrl}/label/receipt/RC-001?receipt_size=80mm", $"{BaseUrl}/label/receipt/RC-002"],
            handler.Calls.Select(c => c.Url));
        Assert.All(handler.Calls, c => Assert.Equal("GET", c.Method));
        Assert.All(handler.Calls, c => Assert.Equal(Auth, c.Headers["Authorization"]));
        Assert.All(handler.Calls, c => Assert.Null(c.Body));
    }
}
