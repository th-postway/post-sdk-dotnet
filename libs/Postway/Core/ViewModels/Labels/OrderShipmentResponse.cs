namespace Postway.Core.ViewModels.Labels;

public class OrderShipmentResponse
{
    public byte[] content { get; set; } = [];
    public int content_length { get; set; }
    public string content_type { get; set; } = "";
    public string file_name { get; set; } = "";
}