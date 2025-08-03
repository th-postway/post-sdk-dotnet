namespace Postway.ViewModels.OrderShipments;

public class FilterRequest
{
    public string filter { get; set; } = "";
    public int limit { get; set; }
    public int page { get; set; }
}