# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

# ThPostway.PostSdk

.NET SDK for the Postway Merchant API. Targets `net8.0`, **zero runtime dependencies** (framework `HttpClient` + `System.Text.Json`). Port of the Node SDK `@th-postway/post-sdk`, which is the behavioural source of truth: same endpoints, wire JSON, environments, error semantics and security rules, with .NET naming.

## Commands

- `dotnet format --verify-no-changes && dotnet build -warnaserror && dotnet test tests/UnitTest` — the check. Run before every commit. `dotnet format` fixes formatting.
- `dotnet test tests/UnitTest` — unit tests (stubbed `HttpMessageHandler`, no network).
- `dotnet test tests/UnitTest --filter "FullyQualifiedName~ReceiptsTests"` — one class; append `.MethodName` for one test.
- `dotnet pack libs/Postway -c Release -o artifacts` — builds the `.nupkg` + `.snupkg`.
- `dotnet test tests/IntegrationTest` — read-only live tests; skipped without `POSTWAY_MERCHANT_BASE_URL` and `POSTWAY_MERCHANT_ACCESS_TOKEN`.
- `POSTWAY_ACCESS_TOKEN=... dotnet run --project demo` — console walkthrough against sandbox; read-only unless `POSTWAY_DEMO_CREATE=1`. `Console`/env reads are fine in `demo/`, never in `libs/`.

## Layout

```
libs/Postway/Core/        PostwayMerchantClient, PostwayMerchantClientOptions, AccessToken, MerchantEnvironment, RequestOptions, PostwaySdk, Errors/
                          internal HttpPipeline, HttpCall, PathSegment, Validation, AccessTokenManager
libs/Postway/Resources/   one class per API area (AuthResource, OrderShipmentsResource, ...)
libs/Postway/Models/      one file per area plus Enums.cs and Common.cs
tests/UnitTest/Support    Stub.cs: StubHandler, Setup(), Json(), Empty(), Text(), ApiError(), Envelope(), Only(), JsonEqual(); FakeTimeProvider
tests/UnitTest/Core       client and pipeline tests
tests/UnitTest/Resources  one file per resource
tests/IntegrationTest     live, read-only
demo/                     runnable Quick start (in the solution, not packed)
```

## How a call flows

1. A resource method builds an `HttpCall`: method, `PathSegment[]` (string literals convert implicitly; caller input via `PathSegment.Param`), optional `Query`, `Body`, `Auth = true` for authenticated routes, and the caller's `RequestOptions`.
2. It hands the call to one of three `HttpPipeline` modes, picked by the endpoint's response shape:
   - `RequestAsync<T>` — plain JSON body.
   - `RequestTextAsync` — HTML/text (a JSON string is unwrapped).
   - `RequestEnvelopeAsync<T>` — `{ code, isSuccess, message, data }`; returns `data`, and a 2xx with `isSuccess: false` throws `PostwayBusinessException`.
3. `SendCheckedAsync` resolves the token through `AccessTokenManager`. With an `AccessTokenProvider`, it learns an unknown lifetime by probing `auth/account/info` (`session.expired`), refreshes after 75% of the lifetime, and replays once after a 403.
4. Errors: `PostwayConfigException` for bad options or a missing token; `PostwayRequestException` for transport failures, timeouts and refused redirects; `PostwayApiException` for non-2xx (`Body` holds the raw response); `PostwayBusinessException` (a subclass of `PostwayApiException`) for envelope failures. Every error reports `RouteUrl`, the route template with `:name` placeholders, never the real URL.

## Rules

- Namespaces: `Postway` (client, options, errors), `Postway.Resources`, `Postway.Models`.
- `PackageSurfaceTests` snapshots the public type list. Change both together, deliberately.
- Never add a runtime dependency.
- `PostwaySdk.Version` must equal `<Version>` in `libs/Postway/Postway.csproj` (test-enforced). The major tracks the runtime (`8.x` → `net8.0`).
- Model properties are PascalCase with explicit `[JsonPropertyName("snake_case")]`. Keep the API's model names in XML-doc `<c>` tags.
- Request models mark required fields `required` and leave optional ones nullable; serialization uses `WhenWritingNull`, so unset optionals stay off the wire. Wire string enums are `const string` classes; model properties stay `string`.
- Every public member needs XML docs: `GenerateDocumentationFile` plus warnings-as-errors makes a missing doc a build failure.
- Every endpoint gets: a resource method, models, a row in the README method catalogue, a line under `Unreleased` in `CHANGELOG.md`, and a unit test asserting method, URL, headers and body.
- Formatting is `dotnet format` with `.editorconfig`. Warnings are errors (`AnalysisLevel` latest-recommended, set in `Directory.Build.props`).
- Work branches start from `develop`, and pull requests target `develop`.

## Security rules (non-negotiable)

- `PostwayConfigException` messages never include the offending value.
- Caller-supplied path segments are wrapped in `PathSegment.Param(name, value)` so they are validated and appear as `:name` in error URLs and messages.
- All header values pass through `Core/Validation.cs` (regexes anchored with `\z`). `BaseUrl` must be https (http only for loopback), with no credentials, query or fragment.
- `AllowAutoRedirect = false` and the 3xx refusal stay. No retries, except the single replay of an authenticated call after a 403 when `AccessTokenProvider` refreshed the token (`Core/AccessTokenManager.cs`).
- No `Console.*`, no `Environment.GetEnvironmentVariable` in `libs/`.
- No tokens, tracking numbers, refs or response bodies in exception messages. `PostwayApiException.Body` stays `[JsonIgnore]` and out of `Message`.
- No internal infrastructure names (hosts, ports, service/framework names, private package names) anywhere in code, comments, tests or docs. Public hosts are only the two in `MerchantBaseUrls`.

## Release

git-flow with default settings; versions are SemVer `MAJOR.MINOR.PATCH` with no `v` prefix. `git flow release start 8.x.y` from `develop`, bump `<Version>` + `PostwaySdk.Version`, update `CHANGELOG.md`, commit, push the release branch (CI checks branch == version), `git flow release finish 8.x.y` (merges to `main`, tags `8.x.y`, merges back to `develop`), then `git push --atomic origin main develop 8.x.y`. The Publish workflow runs only for `MAJOR.MINOR.PATCH` tags, checks the tag is on `main` and equals the version, runs the check, packs and pushes to nuget.org. `ci.yml` is the only build workflow. Full steps in CONTRIBUTING.md.
