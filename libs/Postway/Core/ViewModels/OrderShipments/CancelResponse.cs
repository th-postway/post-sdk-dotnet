namespace Postway.Core.ViewModels.OrderShipments;

public class CancelResponse
{
    public int code { get; set; }
    public bool isSuccess { get; set; }
    public string message { get; set; } = "";
    public object? data { get; set; }
}