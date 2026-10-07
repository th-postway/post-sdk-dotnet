namespace Postway.Models;

// Enum values used on the Merchant API wire, mirrored from the Merchant API's published schema.
// String enums are constant classes and the model properties stay `string`, so a value added on the
// server never breaks deserialization.

/// <summary>Label paper size, for <c>Labels.OrderShipmentsAsync</c>.</summary>
public static class LabelSize
{
    /// <summary><c>4x3</c></summary>
    public const string Size4x3 = "4x3";

    /// <summary><c>4x4</c></summary>
    public const string Size4x4 = "4x4";

    /// <summary><c>4x6</c></summary>
    public const string Size4x6 = "4x6";

    /// <summary><c>6x4</c></summary>
    public const string Size6x4 = "6x4";

    /// <summary><c>A4</c></summary>
    public const string SizeA4 = "A4";

    /// <summary><c>80mm</c></summary>
    public const string Size80mm = "80mm";
}

/// <summary>Label orientation, for <c>Labels.OrderShipmentsAsync</c>.</summary>
public static class LabelOrientation
{
    /// <summary><c>Portrait</c></summary>
    public const string Portrait = "Portrait";

    /// <summary><c>Landscape</c></summary>
    public const string Landscape = "Landscape";
}

/// <summary>Receipt paper width, for <c>Labels.ReceiptAsync</c>.</summary>
public static class ReceiptSize
{
    /// <summary><c>58mm</c></summary>
    public const string R58mm = "58mm";

    /// <summary><c>80mm</c></summary>
    public const string R80mm = "80mm";
}

/// <summary>Parcel content category (numeric on the wire), for <see cref="MerchantOrderShipmentCreatePackage.Type"/>.</summary>
public enum FlashArticleCategory
{
    /// <summary>0</summary>
    File = 0,

    /// <summary>1</summary>
    DryFood = 1,

    /// <summary>2</summary>
    Commodity = 2,

    /// <summary>3</summary>
    DigitalProduct = 3,

    /// <summary>4</summary>
    Clothes = 4,

    /// <summary>5</summary>
    Books = 5,

    /// <summary>6</summary>
    AutoParts = 6,

    /// <summary>7</summary>
    ShoesAndBags = 7,

    /// <summary>8</summary>
    SportsEquipment = 8,

    /// <summary>9</summary>
    Cosmetics = 9,

    /// <summary>10</summary>
    Household = 10,

    /// <summary>11</summary>
    Fruit = 11,

    /// <summary>99</summary>
    Others = 99,
}

/// <summary>Order lifecycle status.</summary>
public static class OrderStatus
{
    /// <summary><c>pending</c></summary>
    public const string Pending = "pending";

    /// <summary><c>on processing</c></summary>
    public const string OnProcess = "on processing";

    /// <summary><c>completed</c></summary>
    public const string Completed = "completed";
}

/// <summary>Parcel (shipment) status.</summary>
public static class OrderShipmentStatus
{
    /// <summary><c>Prepared</c></summary>
    public const string Prepared = "Prepared";

    /// <summary><c>WaitForDropOff</c></summary>
    public const string WaitForDropOff = "WaitForDropOff";

    /// <summary><c>In-Transit</c></summary>
    public const string InTransit = "In-Transit";

    /// <summary><c>Cancel</c></summary>
    public const string Cancel = "Cancel";

    /// <summary><c>Complete</c></summary>
    public const string Complete = "Complete";

    /// <summary><c>Reject</c></summary>
    public const string Reject = "Reject";

    /// <summary><c>Claim</c></summary>
    public const string Claim = "Claim";
}

/// <summary>Channel an order shipment was created through; Merchant API orders are <c>API</c>.</summary>
public static class OrderShipmentChannel
{
    /// <summary><c>V1</c></summary>
    public const string V1 = "V1";

    /// <summary><c>V2</c></summary>
    public const string V2 = "V2";

    /// <summary><c>V3</c></summary>
    public const string V3 = "V3";

    /// <summary><c>UPLOAD_SHEET</c></summary>
    public const string UploadSheet = "UPLOAD_SHEET";

    /// <summary><c>API</c></summary>
    public const string API = "API";
}

/// <summary>How the store pays for shipping.</summary>
public static class StoreBillingType
{
    /// <summary><c>Top Up</c></summary>
    public const string TopUp = "Top Up";

    /// <summary><c>Monthly</c></summary>
    public const string Monthly = "Monthly";
}

/// <summary>How the store receives COD money.</summary>
public static class StoreCodType
{
    /// <summary><c>receipt</c></summary>
    public const string Receipt = "receipt";

    /// <summary><c>billing_transfer</c></summary>
    public const string BillingTransfer = "billing_transfer";
}

/// <summary>Role of the user behind the session.</summary>
public static class UserRole
{
    /// <summary><c>Admin</c></summary>
    public const string Admin = "Admin";

    /// <summary><c>User</c></summary>
    public const string User = "User";

    /// <summary><c>Public</c></summary>
    public const string Public = "Public";
}

/// <summary>Price line description (Thai text on the wire).</summary>
public static class PriceInfoDescription
{
    /// <summary><c>ค่าขนส่ง</c></summary>
    public const string Shipment = "ค่าขนส่ง";

    /// <summary><c>ค่าประกัน</c></summary>
    public const string Insurance = "ค่าประกัน";

    /// <summary><c>ค่าพื้นที่ท่องเที่ยว</c></summary>
    public const string Tourist = "ค่าพื้นที่ท่องเที่ยว";

    /// <summary><c>ค่าพื้นที่เกาะ</c></summary>
    public const string IsLand = "ค่าพื้นที่เกาะ";

    /// <summary><c>ค่าพื้นที่ห่างไกล</c></summary>
    public const string RemoteArea = "ค่าพื้นที่ห่างไกล";

    /// <summary><c>ค่า COD</c></summary>
    public const string COD = "ค่า COD";

    /// <summary><c>เติมเงิน</c></summary>
    public const string TopUp = "เติมเงิน";

    /// <summary><c>ค่าสินค้า</c></summary>
    public const string Product = "ค่าสินค้า";

    /// <summary><c>ค่าเรียกรถเข้ารับ</c></summary>
    public const string Pickup = "ค่าเรียกรถเข้ารับ";

    /// <summary><c>อื่นๆ</c></summary>
    public const string Other = "อื่นๆ";

    /// <summary><c>ค่าน้ำมัน</c></summary>
    public const string Oil = "ค่าน้ำมัน";

    /// <summary><c>ค่าบริการเรียกรถ Move</c></summary>
    public const string Move = "ค่าบริการเรียกรถ Move";

    /// <summary><c>ค่าบริการ</c></summary>
    public const string ServiceCharge = "ค่าบริการ";
}

/// <summary>Pricing region: Bangkok or upcountry.</summary>
public static class PlanDetailRegion
{
    /// <summary><c>BKK</c></summary>
    public const string BKK = "BKK";

    /// <summary><c>UPC</c></summary>
    public const string UPC = "UPC";
}

/// <summary>What the price plan was calculated on.</summary>
public static class PlanDetailType
{
    /// <summary><c>weight</c></summary>
    public const string Weight = "weight";

    /// <summary><c>dimension</c></summary>
    public const string Dimension = "dimension";

    /// <summary><c>cubic</c></summary>
    public const string Cubic = "cubic";
}
