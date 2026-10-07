# Demo

`Program.cs` is a runnable version of the [README Quick start](../README.md#quick-start). It walks one Merchant flow with this repository's SDK. The Node SDK (the reference) and the Python SDK ship the same demo.

1. `Health.PingAsync()`: public, so it works without a token.
2. `Auth.AccountInfoAsync()`: store name and token expiry.
3. `ShipmentProviders.AllAsync()`: couriers the store may use.
4. `Thailand.FilterAsync()`: postal areas for zip code `10500`.
5. `OrderShipments.FilterAsync()`: the store's parcel count and the status of the latest five.
6. _Opt-in, sandbox only:_ `OrderShipments.CreateAsync()` followed by `Labels.OrderShipmentsAsync()`, which saves the label to `demo/output/`.

The demo is **read-only by default** and targets **sandbox**, unlike the SDK, which defaults to production.

## Requirements

- .NET 8 SDK (`global.json`).
- A merchant session access token. Postway issues it to your store; there is no token endpoint.

## Run

```bash
export POSTWAY_ACCESS_TOKEN=...      # from your shell or secret manager, never committed
dotnet run --project demo
```

`demo/Postway.Demo.csproj` references `libs/Postway/Postway.csproj` directly, so it always runs against the source in this repository.

| Variable                    | Required      | Meaning                                                                                         |
| --------------------------- | ------------- | ----------------------------------------------------------------------------------------------- |
| `POSTWAY_ACCESS_TOKEN`      | for steps 2–6 | Merchant session token. `POSTWAY_MERCHANT_ACCESS_TOKEN` (used by integration tests) also works |
| `POSTWAY_MERCHANT_BASE_URL` | no            | Base URL override. Default: `MerchantBaseUrls.Sandbox`                                          |
| `POSTWAY_DEMO_CREATE`       | no            | `1` runs step 6                                                                                 |

Without a token the demo pings, prints which variable to set, and exits with code 1. A 403 means the token is missing, unknown or expired.

A real application would read the token from configuration (`configuration["Postway:AccessToken"]`, user secrets or a secret manager), as in the Quick start. The demo reads environment variables so it needs no extra packages.

## Creating a parcel (opt-in)

```bash
POSTWAY_DEMO_CREATE=1 dotnet run --project demo
```

Step 6 creates a **real sandbox parcel** from the Quick start payload. The courier is the first one `ShipmentProviders.AllAsync()` returns, and `MyTrackingNo` is generated as `DEMO-<unix ms>`. `CreateAsync` is not idempotent, so every run creates a new parcel. The demo refuses this step unless the base URL is `MerchantBaseUrls.Sandbox`. The label is written to `demo/output/` (gitignored).

## Using the local SDK in another project

```bash
dotnet add reference ../post-sdk-dotnet/libs/Postway/Postway.csproj
# or pack it and add a local package source:
dotnet pack ../post-sdk-dotnet/libs/Postway -c Release -o ../local-feed
dotnet add package ThPostway.PostSdk --source ../local-feed
```

## Checks

The demo is part of `Postway.sln` (solution folder `demo`), so the check (`dotnet format --verify-no-changes && dotnet build -warnaserror && ...`) formats and builds it with warnings as errors. It is not packable, and `dotnet pack libs/Postway` ships only the SDK.
