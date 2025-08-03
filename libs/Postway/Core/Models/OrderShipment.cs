namespace Postway.Core.Models;

public class OrderShipment
{
    public string channel { get; set; } = "";
    public Sender sender { get; set; } = new Sender();
    public Recipient recipient { get; set; } = new Recipient();
    public Package package { get; set; } = new Package();
    public string order_shipment_status { get; set; } = "";
    public string status { get; set; } = "";
    public DateTime? completed_at { get; set; }
    public DateTime? in_transit_at { get; set; }
    public DateTime created_at { get; set; }
    public DateTime updated_at { get; set; }

    public class Sender
    {
        public string fullname { get; set; } = "";
        public string mobile_phone { get; set; } = "";
        public string address { get; set; } = "";
        public string sub_district { get; set; } = "";
        public string district { get; set; } = "";
        public string province { get; set; } = "";
        public string zip_code { get; set; } = "";
    }

    public class Recipient
    {
        public string fullname { get; set; } = "";
        public string mobile_phone { get; set; } = "";
        public string address { get; set; } = "";
        public string sub_district { get; set; } = "";
        public string district { get; set; } = "";
        public string province { get; set; } = "";
        public string zip_code { get; set; } = "";
    }

    public class Package
    {
        public string my_tracking_no { get; set; } = "";
        public string tracking_no { get; set; } = "";
        public string ref1 { get; set; } = "";
        public string ref2 { get; set; } = "";
        public string ref3 { get; set; } = "";
        public int weight { get; set; } = 0;
        public int width { get; set; } = 0;
        public int length { get; set; } = 0;
        public int height { get; set; } = 0;
    }
}