namespace Postway.Core.ViewModels.OrderShipments;

public class CalculatePriceRequest
{
    public string shipment_name { get; set; } = "";
    public string r_sub_district { get; set; } = "";
    public string r_district { get; set; } = "";
    public string r_province { get; set; } = "";
    public string r_zip_code { get; set; } = "";
    public int p_weight { get; set; }
    public int p_width { get; set; }
    public int p_height { get; set; }
    public int p_length { get; set; }
    public int p_cod { get; set; }
    public int p_insurance { get; set; }
}