# Contributing

## Setup

```bash
dotnet --version   # 8.0.x; global.json pins the 8.0 SDK with rollForward latestFeature
dotnet restore
dotnet format --verify-no-changes && dotnet build -warnaserror && dotnet test tests/UnitTest   # the check
```

Other commands: `dotnet format` (fix formatting), `dotnet pack libs/Postway -c Release -o artifacts`.

## Layout

```
libs/Postway/Core/            PostwayMerchantClient, options, environments, PostwaySdk.Version, Errors/
                              internal: HttpPipeline, HttpCall, PathSegment, Validation
libs/Postway/Resources/       one class per API area; methods map 1:1 to endpoints
libs/Postway/Models/          request/response models, one file per area (plural), plus Enums.cs
tests/UnitTest/Core           client and HTTP-pipeline tests
tests/UnitTest/Resources      one file per resource, asserting method, URL, headers and body
tests/UnitTest/Support        StubHandler and response builders
tests/UnitTest/PackageSurfaceTests.cs   public type snapshot, version check, wire-name checks
tests/IntegrationTest         read-only live checks, skipped without credentials
```

## Conventions

- Models use PascalCase properties with an explicit `[JsonPropertyName("wire_name")]` on every property; keep the API's model name in the XML doc (`<c>MerchantThailand</c>`).
- Request models mark required fields `required` and leave optional ones nullable, so unset optionals are left out of the JSON.
- Wire string enums are `const string` classes; model properties stay `string`.
- Every public member has XML docs (`GenerateDocumentationFile` + warnings as errors).
- `PostwaySdk.Version` must equal `<Version>` in `libs/Postway/Postway.csproj` (enforced by a test).
- No runtime dependencies. Framework `HttpClient` and `System.Text.Json` only.
- Never log, never read environment variables in `libs/`.

### Security rules

- `PostwayConfigException` messages never repeat the offending value.
- Every caller-supplied path segment goes through `PathSegment.Param(name, value)`, so it is validated and shown as `:name` in errors.
- Header values go through the validators in `Core/Validation.cs` (anchor regexes with `\z`, not `$`).
- Keep `AllowAutoRedirect = false` and the 3xx refusal. No retries beyond the single 403 replay after a token refresh.
- No internal hostnames, ports, service names or private package names anywhere in code, comments, tests or docs. Public hosts are only the two in `MerchantBaseUrls`.

## Adding an endpoint

1. Add the request/response models to `libs/Postway/Models/<Area>.cs`.
2. Add the method to `libs/Postway/Resources/<Area>Resource.cs`; wrap path parameters in `PathSegment.Param()`.
3. Add a test to `tests/UnitTest/Resources/<Area>Tests.cs` asserting method, URL, headers and body.
4. Update the type snapshot in `PackageSurfaceTests.cs` if you added public types.
5. Add a row to the method catalogue in `README.md` and a line under `Unreleased` in `CHANGELOG.md`.

## Integration tests

Read-only, skipped unless both variables are set:

```bash
POSTWAY_MERCHANT_BASE_URL=https://sandbox-post.postway.co.th/merchant \
POSTWAY_MERCHANT_ACCESS_TOKEN=... \
dotnet test tests/IntegrationTest
```

## Releasing

1. Bump `<Version>` in `libs/Postway/Postway.csproj` and `PostwaySdk.Version` in `libs/Postway/Core/PostwaySdk.cs`.
2. Move the `Unreleased` entries in `CHANGELOG.md` under the new version with today's date.
3. Commit, then tag and push: `git tag v8.x.y && git push origin main v8.x.y`.
4. The `Publish` workflow verifies the tag matches the version, runs the check, packs, and pushes the `.nupkg` and `.snupkg` to nuget.org. It needs a `NUGET_API_KEY` repository secret (scoped to `ThPostway.PostSdk`); switch to NuGet trusted publishing once the package exists.
