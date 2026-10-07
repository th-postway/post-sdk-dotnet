# ThPostway.PostSdk

.NET SDK for the Postway Merchant API. Targets `net8.0`, **zero runtime dependencies** (framework `HttpClient` + `System.Text.Json`). Port of the Node SDK `@th-postway/post-sdk`, which is the behavioural source of truth: same endpoints, wire JSON, environments, error semantics and security rules, with .NET naming.

## Commands

- `dotnet format --verify-no-changes && dotnet build -warnaserror && dotnet test tests/UnitTest` — the check. Run before every commit.
- `dotnet test tests/UnitTest` — unit tests (stubbed `HttpMessageHandler`, no network).
- `dotnet pack libs/Postway -c Release -o artifacts` — builds the `.nupkg` + `.snupkg`.
- `dotnet test tests/IntegrationTest` — read-only live tests; skipped without `POSTWAY_MERCHANT_BASE_URL` and `POSTWAY_MERCHANT_ACCESS_TOKEN`.

## Layout

```
libs/Postway/Core/        PostwayMerchantClient, PostwayMerchantClientOptions, MerchantEnvironment, RequestOptions, PostwaySdk, Errors/
                          internal HttpPipeline, HttpCall, PathSegment, Validation
libs/Postway/Resources/   one class per API area (AuthResource, OrderShipmentsResource, ...)
libs/Postway/Models/      one file per area plus Enums.cs and Common.cs
tests/UnitTest/Support    Stub.cs: StubHandler, Setup(), Json(), Empty(), Text(), ApiError(), Envelope(), Only(), JsonEqual()
tests/UnitTest/Core       client and pipeline tests
tests/UnitTest/Resources  one file per resource
tests/IntegrationTest     live, read-only
```

## Rules

- Namespaces: `Postway` (client, options, errors), `Postway.Resources`, `Postway.Models`.
- `PackageSurfaceTests` snapshots the public type list. Change both together, deliberately.
- Never add a runtime dependency.
- `PostwaySdk.Version` must equal `<Version>` in `libs/Postway/Postway.csproj` (test-enforced). The major tracks the runtime (`8.x` → `net8.0`).
- Model properties are PascalCase with explicit `[JsonPropertyName("snake_case")]`. Keep the API's model names in XML-doc `<c>` tags.
- Every endpoint gets: a resource method, models, a row in the README method catalogue, and a unit test asserting method, URL, headers and body.
- Formatting is `dotnet format` with `.editorconfig`. Warnings are errors.

## Security rules (non-negotiable)

- `PostwayConfigException` messages never include the offending value.
- Caller-supplied path segments are wrapped in `PathSegment.Param(name, value)` so they are validated and appear as `:name` in error URLs and messages.
- All header values pass through `Core/Validation.cs` (regexes anchored with `\z`). `BaseUrl` must be https (http only for loopback), with no credentials, query or fragment.
- `AllowAutoRedirect = false` and the 3xx refusal stay. No retries on any call.
- No `Console.*`, no `Environment.GetEnvironmentVariable` in `libs/`.
- No tokens, tracking numbers, refs or response bodies in exception messages. `PostwayApiException.Body` stays `[JsonIgnore]` and out of `Message`.
- No internal infrastructure names (hosts, ports, service/framework names, private package names) anywhere in code, comments, tests or docs. Public hosts are only the two in `MerchantBaseUrls`.

## Release

Bump `<Version>` + `PostwaySdk.Version`, update `CHANGELOG.md`, commit, tag `v8.x.y`, push the tag. The Publish workflow checks tag == version, runs the check, packs and pushes to nuget.org.
