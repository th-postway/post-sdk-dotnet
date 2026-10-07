// Runnable version of the README Quick start. See demo/README.md.
//
//   dotnet run --project demo
//
// Read-only by default and pointed at sandbox. Creating a parcel and printing its label needs
// POSTWAY_DEMO_CREATE=1 and only ever runs against the sandbox base URL.

using System.Runtime.CompilerServices;
using Postway;
using Postway.Models;

var accessToken = FirstSet("POSTWAY_ACCESS_TOKEN", "POSTWAY_MERCHANT_ACCESS_TOKEN");
var baseUrl = FirstSet("POSTWAY_MERCHANT_BASE_URL") ?? MerchantBaseUrls.Sandbox;
var createEnabled = Environment.GetEnvironmentVariable("POSTWAY_DEMO_CREATE") == "1";
var outputDir = Path.Combine(DemoDirectory(), "output");

// never hardcode the token
using var postway = new PostwayMerchantClient(new PostwayMerchantClientOptions { BaseUrl = baseUrl, AccessToken = accessToken });

try
{
    return await RunAsync();
}
catch (PostwayBusinessException error)
{
    Console.Error.WriteLine($"Refused by the API: {string.Join("; ", error.Messages)}");
}
catch (PostwayApiException error) when (error.Status == 403)
{
    Console.Error.WriteLine("HTTP 403: the access token is missing, unknown or expired.");
}
return 1;

async Task<int> RunAsync()
{
    // 1. Public endpoint: works without a token.
    Console.WriteLine($"Base URL: {postway.BaseUrl}");
    Console.WriteLine($"Health.PingAsync -> {await postway.Health.PingAsync()}");

    if (accessToken is null)
    {
        Console.Error.WriteLine("Set POSTWAY_ACCESS_TOKEN (or POSTWAY_MERCHANT_ACCESS_TOKEN) to run the authenticated steps.");
        return 1;
    }

    // 2. Who am I?
    var account = await postway.Auth.AccountInfoAsync();
    Console.WriteLine($"Store: {account.Store.Name}, token expires {account.Session.Expired:O}");

    // 3. Couriers this store may use.
    var providers = await postway.ShipmentProviders.AllAsync();
    Console.WriteLine($"Couriers: {(providers.Count == 0 ? "(none)" : string.Join(", ", providers.Select(provider => provider.Name)))}");

    // 4. Thai postal areas for one zip code.
    var areas = await postway.Thailand.FilterAsync(new MerchantThailandFilterRequest { Page = 1, Limit = 5, ZipCode = "10500" });
    foreach (var area in areas.Data)
    {
        Console.WriteLine($"Area: {area.SubDistrict} / {area.District} / {area.Province} {area.Zipcode}");
    }

    // 5. The store's latest parcels (status only).
    var parcels = await postway.OrderShipments.FilterAsync(new MerchantOrderShipmentFilterRequest { Page = 1, Limit = 5 });
    Console.WriteLine($"Parcels: {parcels.Count} in total");
    foreach (var row in parcels.Data)
    {
        Console.WriteLine($"  - {row.OrderShipmentStatus}");
    }

    if (!createEnabled)
    {
        Console.WriteLine("Read-only run complete. Set POSTWAY_DEMO_CREATE=1 to create a sandbox parcel and print its label.");
        return 0;
    }
    if (postway.BaseUrl != MerchantBaseUrls.Sandbox)
    {
        Console.Error.WriteLine("POSTWAY_DEMO_CREATE=1 only runs against the sandbox base URL; refusing to create a parcel.");
        return 1;
    }

    // 6. Create a parcel (not idempotent: the SDK never retries it).
    var created = await postway.OrderShipments.CreateAsync(new MerchantOrderShipmentCreateRequest
    {
        Shipping = new()
        {
            ShipmentProviderName = providers.Count > 0 ? providers[0].Name : "Flash",
            MyTrackingNo = $"DEMO-{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}",
        },
        Sender = new()
        {
            Fullname = "My Shop",
            MobilePhone = "0811111111",
            Address = "1 Silom Rd",
            SubDistrict = "สีลม",
            District = "บางรัก",
            Province = "กรุงเทพมหานคร",
            ZipCode = "10500",
        },
        Recipient = new()
        {
            Fullname = "Customer",
            MobilePhone = "0822222222",
            Address = "2 Nimman Rd",
            SubDistrict = "สุเทพ",
            District = "เมืองเชียงใหม่",
            Province = "เชียงใหม่",
            ZipCode = "50200",
        },
        Package = new() { InsuranceValue = 0, Weight = 500, Width = 10, Length = 20, Height = 5 },
        ProductCods = [], // empty = not COD
    });
    var parcel = created[0];
    Console.WriteLine($"Created parcel {parcel.Package.MyTrackingNo} ({parcel.OrderShipmentStatus})");

    // 7. Print its label.
    var label = await postway.Labels.OrderShipmentsAsync(new MerchantLabelOrderShipmentsRequest
    {
        TrackingNos = [parcel.Package.TrackingNo],
        LabelSize = LabelSize.Size4x6,
        LabelOrientation = LabelOrientation.Portrait,
    });
    Directory.CreateDirectory(outputDir);
    // Path.GetFileName keeps a server-supplied name from escaping the output directory.
    var labelPath = Path.GetFullPath(Path.Combine(outputDir, Path.GetFileName(label.FileName)));
    await File.WriteAllBytesAsync(labelPath, label.DecodeContent());
    Console.WriteLine($"Label saved to {labelPath}");
    return 0;
}

static string? FirstSet(params string[] names) =>
    names.Select(Environment.GetEnvironmentVariable).FirstOrDefault(value => !string.IsNullOrEmpty(value));

// The directory of this source file, so the label lands in demo/output/ wherever `dotnet run` is started.
static string DemoDirectory([CallerFilePath] string path = "") => Path.GetDirectoryName(path)!;
