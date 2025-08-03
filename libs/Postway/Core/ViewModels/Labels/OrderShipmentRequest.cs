using Postway.Core.CONST;

namespace Postway.Core.ViewModels.Labels;

public class OrderShipmentRequest
{
    public IEnumerable<string> tracking_nos { get; set; } = [];
    public string label_size { get; set; } = VALUE.LABEL_SIZE.SIX_BY_FOUR;
    public string label_orientation { get; set; } = VALUE.LABEL_ORIENTATION.PORTRAIT;
}