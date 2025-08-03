namespace Postway.Core.ViewModels.OrderShipments;

public class CreateRequest
{
    public Shipping shipping { get; set; } = new Shipping();
    public Sender sender { get; set; } = new Sender();
    public Recipient recipient { get; set; } = new Recipient();
    public Package package { get; set; } = new Package();
    public List<ProductCod> product_cods { get; set; } = [];

    public class Shipping
    {
        public string shipment_provider_name { get; set; } = "";
        public string my_tracking_no { get; set; } = "";
    }

    public class Sender
    {
        public string fullname { get; set; } = "";
        public string email { get; set; } = "";
        public string mobile_phone { get; set; } = "";
        public string card_no { get; set; } = "";
        public string address { get; set; } = "";
        public string sub_district { get; set; } = "";
        public string district { get; set; } = "";
        public string province { get; set; } = "";
        public string zip_code { get; set; } = "";
    }

    public class Recipient
    {
        public string fullname { get; set; } = "";
        public string email { get; set; } = "";
        public string mobile_phone { get; set; } = "";
        public string address { get; set; } = "";
        public string sub_district { get; set; } = "";
        public string district { get; set; } = "";
        public string province { get; set; } = "";
        public string zip_code { get; set; } = "";
    }

    public class Package
    {
        public int insurance_value { get; set; } = 0;
        public string note { get; set; } = "";
        public int type { get; set; } = 0;
        public int weight { get; set; } = 0;
        public int width { get; set; } = 0;
        public int height { get; set; } = 0;
        public int length { get; set; } = 0;
    }

    public class ProductCod
    {
        public string name { get; set; } = "";
        public int amount { get; set; }
        public int price_per_item { get; set; }
    }
}