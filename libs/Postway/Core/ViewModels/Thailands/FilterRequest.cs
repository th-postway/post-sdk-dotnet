namespace Postway.Core.ViewModels.Thailands;

public class FilterRequest
{
    public string filter { get; set; } = "";
    public int limit { get; set; }
    public int page { get; set; }
    public List<string> shipment_provider_names { get; set; } = [];
    public string sub_district { get; set; } = "";
    public string district { get; set; } = "";
    public string province { get; set; } = "";
    public string zip_code { get; set; } = "";
}