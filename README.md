# ThPostway.PostSdk

.NET SDK for the **Postway Merchant API**: create and track parcels, quote prices, print labels and receipts, and look up Thai postal areas. It covers every live Merchant endpoint, has **no runtime dependencies** (`HttpClient` and `System.Text.Json` from the framework), and ships nullable-annotated XML-documented APIs.

- [Requirements](#requirements)
- [Install](#install)
- [Quick start](#quick-start)
- [Demo](#demo)
- [Authentication](#authentication)
- [Environments](#environments)
- [Method catalogue](#method-catalogue)
- [Errors](#errors)
- [Labels and receipt files](#labels-and-receipt-files)
- [Security](#security)
- [Units and conventions](#units-and-conventions)
- [Development](#development)
- [Mapping from the Node SDK](#mapping-from-the-node-sdk)
- [Mapping from the legacy `Post` / `IPost` API](#mapping-from-the-legacy-post--ipost-api)

## Requirements

**.NET 8** (`net8.0`) or later. The package major version tracks the target runtime: `8.x` targets `net8.0`.

## Install

```bash
dotnet add package ThPostway.PostSdk
```

## Quick start

```csharp
using Postway;
using Postway.Models;

using var postway = new PostwayMerchantClient(new PostwayMerchantClientOptions
{
    AccessToken = configuration["Postway:AccessToken"], // never hardcode it
    Environment = MerchantEnvironment.Production,        // default
});

var account = await postway.Auth.AccountInfoAsync();
Console.WriteLine($"{account.Store.Name} token expires {account.Session.Expired}");

var created = await postway.OrderShipments.CreateAsync(new MerchantOrderShipmentCreateRequest
{
    Shipping = new() { ShipmentProviderName = "Flash", MyTrackingNo = "ORDER-1001" },
    Sender = new()
    {
        Fullname = "My Shop", MobilePhone = "0811111111", Address = "1 Silom Rd",
        SubDistrict = "สีลม", District = "บางรัก", Province = "กรุงเทพมหานคร", ZipCode = "10500",
    },
    Recipient = new()
    {
        Fullname = "Customer", MobilePhone = "0822222222", Address = "2 Nimman Rd",
        SubDistrict = "สุเทพ", District = "เมืองเชียงใหม่", Province = "เชียงใหม่", ZipCode = "50200",
    },
    Package = new() { InsuranceValue = 0, Weight = 500, Width = 10, Length = 20, Height = 5 },
    ProductCods = [], // empty = not COD
});

var label = await postway.Labels.OrderShipmentsAsync(new MerchantLabelOrderShipmentsRequest
{
    TrackingNos = [created[0].Package.TrackingNo],
    LabelSize = LabelSize.Size4x6,
    LabelOrientation = LabelOrientation.Portrait,
});
// Path.GetFileName keeps a server-supplied name from escaping the current directory.
await File.WriteAllBytesAsync(Path.GetFileName(label.FileName), label.DecodeContent());
```

`PostwayMerchantClient` is thread-safe. Create one per access token and reuse it; dispose it when you are done (it owns its `HttpClient` unless you inject one).

## Demo

[`demo/Program.cs`](demo/README.md) is the Quick start as a console app you can run from a clone of this repository. It pings, reads account info, couriers, Thai postal areas and the store's parcels. It targets **sandbox** and is **read-only** unless you set `POSTWAY_DEMO_CREATE=1`, which also creates a sandbox parcel and saves its label. The token comes from the environment only.

```bash
POSTWAY_ACCESS_TOKEN=... dotnet run --project demo
```

See [demo/README.md](demo/README.md) for every variable and for using the local SDK in another project. The Node and Python SDKs ship the same demo. The demo project is not packable and is not part of the NuGet package.

## Authentication

The Merchant API uses a **merchant session access token**. Postway issues it to your store out of band; there is no token endpoint in the API. The SDK sends it as:

```
Authorization: Bearer <AccessToken>
```

- The server looks up the session by token type plus token, so set `TokenType` only if Postway gave you a different type. The default is `"Bearer"`.
- Every call acts as the **owner of the store** the token belongs to, and every query is scoped to that store.
- A missing, unknown or **expired** token gets HTTP **403**. `Auth.AccountInfoAsync()` returns `Session.Expired`; set `AccessTokenProvider` (below) and the SDK rotates the token for you.
- `Receipts.*` and `Health.PingAsync()` are public and never send the token. Every other method throws `PostwayConfigException` before any network call if the client has neither `AccessToken` nor `AccessTokenProvider`.

### Refreshing tokens automatically

The API cannot issue tokens, so you supply them: `AccessTokenProvider` returns an `AccessToken(value, expiresAt?)`, and the SDK decides when to call it.

```csharp
using var postway = new PostwayMerchantClient(new PostwayMerchantClientOptions
{
    AccessTokenProvider = async (reason, cancellationToken) =>
    {
        // reason: AccessTokenRefreshReason.Initial | Expiring | Forbidden
        var (token, expiresAt) = await secretStore.GetPostwayTokenAsync(cancellationToken);
        return new AccessToken(token, expiresAt); // ExpiresAt is optional
    },
});
```

- **When it is called**: once for the first token (`Initial`), when **75% of the token's lifetime has elapsed** (less than 25% left, `Expiring`), and after a **403** (`Forbidden`). If both `AccessToken` and `AccessTokenProvider` are set, the static token is used first.
- **Lifetime** comes from the first source available: the `ExpiresAt` you return, else the JWT `exp` / `iat` claims (decoded locally; the signature is not checked), else one `POST auth/account/info` probe per token that reads `session.expired`. Your own `Auth.AccountInfoAsync()` calls update it as well. A probe answered with 403 refreshes straight away; any other probe failure is ignored and the call goes ahead.
- **403 replay**: when an authenticated call gets a 403, the SDK refreshes once and sends the same request once more. A second 403 throws `PostwayApiException` as usual, so it never loops. The API rejects the token before the request runs, so the replay is safe for `CreateAsync` and `CancelAsync` too.
- Concurrent calls share one provider call and one probe. Exceptions thrown by the provider propagate unchanged, and nothing is sent. A returned token that is not a safe header value throws `PostwayConfigException` without echoing it. `AccessToken.ToString()` never prints the token.
- `TimeProvider` (default `TimeProvider.System`) is the clock used for the 75% check; inject a fake one in tests.
- Without `AccessTokenProvider` nothing changes: no probe, no refresh, no replay.

## Environments

| `Environment`                                | Base URL                                      |
| -------------------------------------------- | --------------------------------------------- |
| `MerchantEnvironment.Production` _(default)_ | `https://post.postway.co.th/merchant`         |
| `MerchantEnvironment.Sandbox`                | `https://sandbox-post.postway.co.th/merchant` |

The URLs are also available as `MerchantBaseUrls.Production` / `MerchantBaseUrls.Sandbox`. `BaseUrl` overrides `Environment`:

```csharp
new PostwayMerchantClient(new PostwayMerchantClientOptions { BaseUrl = MerchantBaseUrls.Sandbox, AccessToken = token });
```

### Client options

| Option        | Default                                    | Notes                                                                                   |
| ------------- | ------------------------------------------ | --------------------------------------------------------------------------------------- |
| `AccessToken` | —                                          | Merchant session token                                                                  |
| `AccessTokenProvider` | —                                  | `Func<AccessTokenRefreshReason, CancellationToken, ValueTask<AccessToken>>`; turns on automatic refresh (see above) |
| `TimeProvider` | `TimeProvider.System`                     | Clock for token refresh timing                                                          |
| `TokenType`   | `"Bearer"`                                 | Auth scheme word of `Authorization`                                                     |
| `Environment` | `MerchantEnvironment.Production`           | See table above                                                                         |
| `BaseUrl`     | from `Environment`                         | `https://` only (`http://` for localhost); no credentials, query or fragment            |
| `Timeout`     | 60 s                                       | Per request; whole milliseconds, at most `int.MaxValue` ms. Override per call with `RequestOptions` |
| `HttpClient`  | SDK-owned, redirects disabled              | Inject for `IHttpClientFactory`, proxies, tracing or tests. Never disposed by the SDK; must not follow redirects |
| `UserAgent`   | `postway-sdk-dotnet/<ver> dotnet/<ver>`    |                                                                                         |

`new PostwayMerchantClient(accessToken)` is shorthand for a production client with that token.

Every `…Async` method takes two optional trailing parameters: `RequestOptions? options` (`{ Timeout }`) and `CancellationToken cancellationToken`. `Receipts.PublicUrl` is synchronous and makes no request.

The constructor validates every option and throws `PostwayConfigException` for unsafe values. Its messages never repeat the value, so they are safe to log.

### Using `IHttpClientFactory`

```csharp
services.AddHttpClient("postway")
    .ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler { AllowAutoRedirect = false });

services.AddSingleton(sp => new PostwayMerchantClient(new PostwayMerchantClientOptions
{
    AccessToken = sp.GetRequiredService<IConfiguration>()["Postway:AccessToken"],
    HttpClient = sp.GetRequiredService<IHttpClientFactory>().CreateClient("postway"),
}));
```

The SDK applies its own `Timeout` per call; an injected client's `HttpClient.Timeout` still applies on top (100 s by default).

## Method catalogue

Paths are relative to the base URL. All methods are `async` and return `Task`.

| Method                                                              | HTTP                                                 | Auth | Returns                                                                 |
| ------------------------------------------------------------------- | ---------------------------------------------------- | ---- | ----------------------------------------------------------------------- |
| `Auth.AccountInfoAsync()`                                           | `POST auth/account/info`                             | ✓    | `MerchantAuthAccountInfoResponse`: store, owner, `Session.Expired`      |
| `OrderShipments.GetByTrackingNoAsync(trackingNo)`                   | `GET order-shipment/get-by-tracking-no/:tracking_no` | ✓    | `MerchantOrderShipmentData?`                                            |
| `OrderShipments.GetByRefAsync(reference)`                           | `GET order-shipment/get-by-ref/:ref`                 | ✓    | `MerchantOrderShipmentData?`; matches `ref1`, `ref2` or `ref3`          |
| `OrderShipments.FilterAsync(request)`                               | `POST order-shipment/filter`                         | ✓    | `FilterResponse<MerchantOrderShipmentData>`, newest first               |
| `OrderShipments.CreateAsync(request \| requests)`                   | `POST order-shipment/create`                         | ✓    | `IReadOnlyList<MerchantOrderShipmentData>`                              |
| `OrderShipments.CalculatePriceAsync(request)`                       | `POST order-shipment/calculate-price`                | ✓    | `MerchantOrderShipmentCalculatePriceResponse`: `PriceInfos`, `PlanDetail` |
| `OrderShipments.CancelAsync(trackingNo)`                            | `POST order-shipment/cancel`                         | ✓    | —                                                                       |
| `ShipmentProviders.AllAsync()`                                      | `GET shipment-provider/all`                          | ✓    | `IReadOnlyList<MerchantShipmentProviderData>`: couriers this store may use |
| `Thailand.FilterAsync(request)`                                     | `POST thailand/filter`                               | ✓    | `FilterResponse<MerchantThailand>`                                      |
| `Labels.OrderShipmentsAsync(request)`                               | `POST label/order/shipments`                         | ✓    | `FileHttpResponse` (base64)                                             |
| `Labels.ReceiptAsync(receiptNo, receiptSize?)`                      | `GET label/receipt/:receipt_no?receipt_size=`        | ✓    | `FileHttpResponse` (base64)                                             |
| `Receipts.GetPublicAsync(token)`                                    | `GET receipt/public/:token`                          | —    | `PublicReceiptResponse`                                                 |
| `Receipts.GetPublicHtmlAsync(token)`                                | `GET receipt/:token`                                 | —    | HTML `string`                                                           |
| `Receipts.PublicUrl(token)`                                         | _(no request, synchronous)_                          | —    | URL of the public receipt page                                          |
| `Health.PingAsync()`                                                | `GET health/ping`                                    | —    | `"pong"`                                                                |

Behaviour worth knowing:

- **`OrderShipments.CreateAsync`** accepts one request or a sequence; the server always receives an array. Each parcel is priced, verified, created, booked with the courier, and covered by one receipt. It is **not idempotent** and the SDK never retries it (or anything else), except for the single replay after a 403 when `AccessTokenProvider` is set (the server rejected the token, so nothing was created). A batch stops at the first failing parcel, and parcels created before that failure remain. On `PostwayBusinessException`, look them up by `my_tracking_no` (`OrderShipments.FilterAsync`) before you resubmit.
- **`OrderShipments.CancelAsync`** matches the courier `tracking_no` only, not `my_tracking_no` or refs.
- **`Labels.OrderShipmentsAsync`**: each `TrackingNos` entry may be a `tracking_no`, `my_tracking_no` or `ref1..3`. If none match, the server returns 400.
- **`Thailand.FilterAsync`**: the field filters (`SubDistrict`, `District`, `Province`, `ZipCode`) are exact matches. `ShipmentProviderNames` restricts results to areas served by those couriers; leave it `null` for all (the SDK always sends the array, as `[]` when unset). The response spells the zip field `zipcode` (`MerchantThailand.Zipcode`).
- **`ShipmentProviderName` / `ShipmentName`** take a `Name` from `ShipmentProviders.AllAsync()`.
- **`Filter` paging**: `Limit` must be 1..1,000,000 and `Page` ≥ 1. A page past the end wraps to page 1.

Wire enums are constant classes (`LabelSize`, `LabelOrientation`, `ReceiptSize`, `OrderShipmentStatus`, `OrderStatus`, `OrderShipmentChannel`, `StoreBillingType`, `StoreCodType`, `UserRole`, `PriceInfoDescription`, `PlanDetailRegion`, `PlanDetailType`) and the model properties are `string`, so a value added on the server never breaks deserialization. `FlashArticleCategory` is numeric on the wire and a C# `enum`.

```csharp
if (parcel.OrderShipmentStatus == OrderShipmentStatus.InTransit) { /* "In-Transit" */ }
```

## Errors

All SDK exceptions extend `PostwayException`.

| Class                                                         | When                                                                        | Useful properties                                              |
| ------------------------------------------------------------- | --------------------------------------------------------------------------- | -------------------------------------------------------------- |
| `PostwayApiException`                                         | Non-2xx response                                                            | `Status`, `Code`, `Messages`, `Body`, `Method`, `Url`          |
| `PostwayBusinessException` _(extends `PostwayApiException`)_  | 2xx response whose envelope says `isSuccess: false`                         | same                                                           |
| `PostwayRequestException`                                     | Network failure, refused redirect or SDK timeout; nothing usable came back | `InnerException`, `Method`, `Url`                              |
| `PostwayConfigException`                                      | Invalid client options or call arguments, or a guarded call without a token | —                                                              |

Cancelling your own `CancellationToken` throws `OperationCanceledException` (with your token), as usual in .NET; only the SDK's `Timeout` becomes `PostwayRequestException`. A `null` request argument throws `ArgumentNullException`.

The server reports errors as `{ code, isSuccess: false, message, data: null }` with the real HTTP status:

| HTTP | Meaning                                                                                         |
| ---- | ----------------------------------------------------------------------------------------------- |
| 400  | Validation failure (`Messages` may hold several entries) or business rule, e.g. order not found |
| 403  | Missing, unknown or expired token (with `AccessTokenProvider`: still 403 after one refresh)     |
| 404  | Public receipt token invalid                                                                    |
| 500  | Server error; `Messages` is a generic text                                                      |

`Code` in the body is **400 or 500**, not the HTTP status. Use `Status` to branch.

Two details keep error logs safe to keep:

- `Url` is the **route template** (`…/receipt/public/:token`), not the URL that was sent. Receipt tokens, tracking numbers and refs never appear in `Url` or `Message`.
- `Body` (the raw response text) is never part of `Message` or `ToString()`, and is `[JsonIgnore]`. Read `error.Body` explicitly when you need it.

`OrderShipments.CreateAsync` and `CancelAsync` can fail **with HTTP 201** and `isSuccess: false`, for example when verification or the courier rejects the parcel. The SDK turns that into `PostwayBusinessException`, so a completed task always means success.

```csharp
try
{
    await postway.OrderShipments.CancelAsync("TH0001");
}
catch (PostwayBusinessException error)
{
    logger.LogWarning("refused: {Messages}", error.Messages);
}
catch (PostwayApiException error) when (error.Status == 403)
{
    logger.LogWarning("token expired");
}
```

## Labels and receipt files

Label and receipt endpoints return JSON, not raw bytes:

```csharp
public sealed class FileHttpResponse
{
    public string FileName { get; set; }    // file_name
    public string Content { get; set; }     // content, base64
    public string ContentType { get; set; } // content_type
    public long ContentLength { get; set; } // content_length
    public byte[] DecodeContent();
}
```

`DecodeContent()` returns the decoded bytes.

## Security

- **Transport**: `BaseUrl` must be `https://`; plain `http://` is accepted only for `localhost`, `127.0.0.1` and `[::1]`. URLs with credentials, a query string or a fragment are rejected.
- **Headers**: `AccessToken`, `TokenType` and `UserAgent` are checked at construction (printable ASCII, no line breaks — including a trailing one) so a pasted token with a stray line break cannot inject headers or leak into an error message.
- **Paths**: caller-supplied path parameters are percent-encoded (`Uri.EscapeDataString`) and may not be `null`, empty, `.` or `..`, so a bad input cannot reach a different endpoint.
- **Redirects** are refused: the SDK-owned handler has `AllowAutoRedirect = false`, any 3xx response throws `PostwayRequestException`, and so does a response an injected handler reached by following a redirect. The API never redirects, and following one could re-send `Authorization` elsewhere.
- **Retries**: none, except one replay of an authenticated call after a 403 when `AccessTokenProvider` is set. Server errors, timeouts and network failures are never retried; `CreateAsync` is not idempotent.
- **Provider tokens** from `AccessTokenProvider` pass the same header check before use. JWT claims are read only to time a refresh; neither tokens nor claims appear in exceptions.
- **Errors** report route templates instead of parameter values, and response bodies stay out of messages and serialization (see [Errors](#errors)).
- The SDK has **no runtime dependencies**, never logs, and never reads environment variables.
- The access token is held in a private field and is only sent on authenticated routes (the provider is never called for public ones). Store it in a secret manager or user secrets, never in source control.

To report a vulnerability, see [SECURITY.md](SECURITY.md).

## Units and conventions

- Weight is in **grams**. Width, length and height are in **cm**. Money is in **THB**. Numeric wire values are `decimal` (counts are `int`).
- COD amount = Σ `PricePerItem × Amount` over `ProductCods`. An empty list means a non-COD parcel.
- Timestamps are `DateTimeOffset` (nullable where the API may send `null`). The exception is `PublicReceiptResponse.CreatedAt`, which is a pre-formatted `yyyy/MM/dd HH:mm:ss` `string`.
- Properties are PascalCase; each carries `[JsonPropertyName]` with the exact wire name (`snake_case`, plus the API's own `zipcode` and `total_affliliate`), so payloads match the Swagger docs one to one. Unset optional request fields are left out of the JSON.

## Development

```bash
dotnet --version     # 8.0.x (global.json rolls forward to the latest 8.0 feature band)
dotnet format --verify-no-changes && dotnet build -warnaserror && dotnet test tests/UnitTest   # the check
dotnet pack libs/Postway -c Release -o artifacts
dotnet run --project demo   # runnable Quick start (see Demo)
```

```
libs/Postway/Core/        client, options, environments, errors, validation, HTTP pipeline, SDK version
libs/Postway/Resources/   one class per API area (Auth, OrderShipments, Labels, ...)
libs/Postway/Models/      request/response models, one file per area, plus Enums.cs
tests/UnitTest/           Core/, Resources/ (one file per resource), Support/ (stub HttpMessageHandler), PackageSurfaceTests
tests/IntegrationTest/    read-only live checks, skipped without credentials
demo/                     runnable Quick start; in Postway.sln, so the check formats and builds it
```

See [CONTRIBUTING.md](CONTRIBUTING.md) for conventions and the release procedure.

Unit tests (xUnit v3, stubbed `HttpMessageHandler`, no network) assert the exact method, URL, headers and body for every endpoint, plus error mapping, the 201 + `isSuccess:false` case, empty body → `null`, timeouts, cancellation and redirect refusal. `PackageSurfaceTests` snapshots the public type list, checks that `PostwaySdk.Version` equals the package `<Version>`, and pins the JSON wire names of every request model.

Integration tests are **read-only** (ping, account info, couriers, Thailand and parcel filters, unknown tracking number → `null`, invalid token → 403). They are skipped unless both variables are set:

```bash
POSTWAY_MERCHANT_BASE_URL=https://sandbox-post.postway.co.th/merchant \
POSTWAY_MERCHANT_ACCESS_TOKEN=... \
dotnet test tests/IntegrationTest
```

They never create or cancel parcels.

## Mapping from the Node SDK

The Node SDK (`@th-postway/post-sdk`) is the reference implementation; this port has the same endpoints, wire JSON, environments, error semantics and security rules, with .NET naming.

| Node (`@th-postway/post-sdk`)                       | .NET (`ThPostway.PostSdk`)                                                  |
| --------------------------------------------------- | --------------------------------------------------------------------------- |
| `new PostwayMerchantClient({ accessToken, ... })`   | `new PostwayMerchantClient(new PostwayMerchantClientOptions { AccessToken = ..., ... })` |
| `getAccessToken: reason => token \| { accessToken, expiresAt }` | `AccessTokenProvider = (reason, ct) => ValueTask<AccessToken>`; reasons are `AccessTokenRefreshReason` |
| `vi.setSystemTime` in tests                         | `TimeProvider` option                                                        |
| `environment: 'production' \| 'sandbox'`            | `Environment = MerchantEnvironment.Production \| Sandbox`                    |
| `MERCHANT_BASE_URLS`                                | `MerchantBaseUrls.Production`, `.Sandbox`, `.For(env)`                       |
| `timeoutMs: 60000`                                  | `Timeout = TimeSpan.FromSeconds(60)`                                         |
| `fetch`                                             | `HttpClient`                                                                 |
| `{ signal, timeoutMs }` per call                    | `RequestOptions { Timeout }` + `CancellationToken`                           |
| `client.orderShipments.getByTrackingNo(t)`          | `client.OrderShipments.GetByTrackingNoAsync(t)` (every method: PascalCase + `Async`) |
| `labels.receipt(no, { receiptSize })`               | `Labels.ReceiptAsync(no, receiptSize)`                                       |
| `receipts.publicUrl(token)`                         | `Receipts.PublicUrl(token)`                                                  |
| `decodeFile(file)` → `Buffer`                       | `file.DecodeContent()` → `byte[]`                                            |
| snake_case interface fields (`zip_code`)            | PascalCase properties with `[JsonPropertyName("zip_code")]`                  |
| `IsoDateString`                                     | `DateTimeOffset` / `DateTimeOffset?`                                         |
| `LabelSize.Size4x6` (`as const` + union type)       | `LabelSize.Size4x6` (`const string`); `FlashArticleCategory` is an `enum`    |
| `PostwayError` / `PostwayConfigError` / `PostwayApiError` / `PostwayBusinessError` / `PostwayRequestError` | `PostwayException` / `PostwayConfigException` / `PostwayApiException` / `PostwayBusinessException` / `PostwayRequestException` |
| `error.messages`, `error.code`, `error.status`      | `error.Messages`, `error.Code`, `error.Status`                               |
| `error.body` (parsed JSON or text, non-enumerable)  | `error.Body` (raw text, `[JsonIgnore]`, never in `Message`/`ToString()`)     |
| `error.cause`                                       | `error.InnerException`                                                       |
| abort via `signal` → `PostwayRequestError`          | cancel via `CancellationToken` → `OperationCanceledException`                |
| `SDK_VERSION`                                       | `PostwaySdk.Version`                                                         |

Versions: Node `22.x` tracks Node 22; .NET `8.x` tracks `net8.0`.

## Mapping from the legacy `Post` / `IPost` API

Version 8.0.0 replaces the old `Postway.Post` class (version 1.0.0) entirely. The old API sent unencoded path parameters, put tracking numbers in exception messages, opened a new `HttpClient` per call and had no error model.

| Legacy (`Postway.Post` / `IPost`)                         | 8.0.0                                                                  |
| --------------------------------------------------------- | ---------------------------------------------------------------------- |
| `new Post(accessToken)`                                   | `new PostwayMerchantClient(accessToken)`                               |
| `Auth_AccountInfo()` → `AccountInfoResponse`              | `Auth.AccountInfoAsync()` → `MerchantAuthAccountInfoResponse`          |
| `OrderShipment_GetByTrackingNo(trackingNo)` → `GetByTrackingNoResponse` | `OrderShipments.GetByTrackingNoAsync(trackingNo)` → `MerchantOrderShipmentData?` (`null` when not found) |
| `OrderShipment_GetByRef(refNo)` → `GetByRefResponse`      | `OrderShipments.GetByRefAsync(reference)` → `MerchantOrderShipmentData?` |
| `Label_OrderShipment(OrderShipmentRequest)` → `OrderShipmentResponse` (`byte[] content`) | `Labels.OrderShipmentsAsync(MerchantLabelOrderShipmentsRequest)` → `FileHttpResponse` (`DecodeContent()`) |
| `VALUE.LABEL_SIZE.SIX_BY_FOUR`, `VALUE.LABEL_ORIENTATION.PORTRAIT` | `LabelSize.Size6x4`, `LabelOrientation.Portrait`              |
| snake_case properties (`tracking_no`)                     | PascalCase properties (`TrackingNo`), same JSON                        |
| `Exception` / `HttpRequestException`                      | `PostwayApiException`, `PostwayBusinessException`, `PostwayRequestException`, `PostwayConfigException` |
| `HttpClientService`, `JsonExtension`                      | removed (internal pipeline)                                            |
| —                                                         | `OrderShipments.FilterAsync / CreateAsync / CalculatePriceAsync / CancelAsync`, `ShipmentProviders.AllAsync`, `Thailand.FilterAsync`, `Labels.ReceiptAsync`, `Receipts.*`, `Health.PingAsync` |

Both default to `https://post.postway.co.th/merchant`.
