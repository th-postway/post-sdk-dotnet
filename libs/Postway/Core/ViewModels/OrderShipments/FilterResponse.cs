using Postway.Models;

namespace Postway.ViewModels.OrderShipments;

public class FilterResponse
{
    public int count { get; set; }
    public List<OrderShipment> data { get; set; } = [];
    public int limit { get; set; }
    public int page { get; set; }
    public int page_count { get; set; }
}