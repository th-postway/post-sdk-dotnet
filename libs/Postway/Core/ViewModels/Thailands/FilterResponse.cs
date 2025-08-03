namespace Postway.Core.ViewModels.Thailands;

public class FilterResponse
{
    public int count { get; set; }
    public List<Thailand> data { get; set; } = [];
    public int limit { get; set; }
    public int page { get; set; }
    public int page_count { get; set; }

    public class Thailand
    {
        public string sub_district { get; set; } = "";
        public string district { get; set; } = "";
        public string province { get; set; } = "";
        public string zipcode { get; set; } = "";
        public bool is_island { get; set; }
        public bool is_remote_area { get; set; }
        public bool is_tourist { get; set; }
        public List<string> shipment_provider_names { get; set; } = [];
    }
}