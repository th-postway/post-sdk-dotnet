namespace Postway.Core.ViewModels.Auths;

public class AccountInfoResponse
{
    public User user { get; set; } = new User();
    public Store store { get; set; } = new Store();
    public Session session { get; set; } = new Session();

    public class User
    {
        public string username { get; set; } = "";
        public string email { get; set; } = "";
        public string first_name { get; set; } = "";
        public string last_name { get; set; } = "";
        public string mobile_phone { get; set; } = "";
        public string role { get; set; } = "";
    }

    public class Store
    {
        public string code { get; set; } = "";
        public string name { get; set; } = "";
        public string mobile_phone { get; set; } = "";
        public string store_billing_type { get; set; } = "";
        public string store_cod_type { get; set; } = "";
        public string address { get; set; } = "";
        public string sub_district { get; set; } = "";
        public string district { get; set; } = "";
        public string province { get; set; } = "";
        public string zip_code { get; set; } = "";
    }

    public class Session
    {
        public DateTime expired { get; set; }
    }
}



