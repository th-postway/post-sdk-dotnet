namespace Postway.Core.ViewModels.OrderShipments;

public class CalculatePriceResponse
{
    public PlanDetail plan_detail { get; set; } = new PlanDetail();
    public List<PriceInfo> price_infos { get; set; } = [];

    public class PlanDetail
    {
        public string region { get; set; } = "";
        public string type { get; set; } = "";
        public int max_boundary { get; set; }
        public int min_boundary { get; set; }
    }

    public class PriceInfo
    {
        public string description { get; set; } = "";
        public int cost { get; set; }
        public int price { get; set; }
        public int cashback_cost { get; set; }
        public bool is_reward_cashback { get; set; }
        public int total_affliliate { get; set; }
    }
}