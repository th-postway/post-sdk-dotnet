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

## Branches and releases

The repository uses git-flow with its default settings (no tag prefix):

| Branch | Purpose |
| --- | --- |
| `main` | released code; every release is a tag on it |
| `develop` | integration branch; open pull requests against it |
| `feature/*`, `bugfix/*` | work branches, started from `develop` |
| `release/<version>` | release preparation, started from `develop` |
| `hotfix/<version>` | urgent fix to a released version, started from `main` |

Versions are SemVer `MAJOR.MINOR.PATCH` with **no `v` prefix**, used as is in branch and tag names: `release/8.1.0` becomes tag `8.1.0`. The major tracks the runtime (`8.x` → `net8.0`).

CI (`.github/workflows/ci.yml`) is the only build workflow. It runs on pushes to `main`, `develop`, `release/**` and `hotfix/**`, and on every pull request. On `release/*` and `hotfix/*` it also fails unless the branch name is `MAJOR.MINOR.PATCH` and equals `<Version>` in `libs/Postway/Postway.csproj`. Dependabot opens its pull requests against `develop`.

### Releasing

1. `git flow release start 8.x.y` (from `develop`).
2. Bump `<Version>` in `libs/Postway/Postway.csproj` and `PostwaySdk.Version` in `libs/Postway/Core/PostwaySdk.cs`, move the `Unreleased` entries in `CHANGELOG.md` under `[8.x.y]` with today's date, and commit.
3. `git push -u origin release/8.x.y` and wait for CI.
4. `git flow release finish 8.x.y`: merges into `main`, tags `8.x.y` there, and merges back into `develop`.
5. `git push --atomic origin main develop 8.x.y`, then delete `release/8.x.y` on the remote if it is still there.

For an urgent fix, run the same steps with `git flow hotfix start 8.x.y` (from `main`) and `git flow hotfix finish 8.x.y`.

The `Publish` workflow (`.github/workflows/publish.yml`) runs only for tags matching `MAJOR.MINOR.PATCH`; a `v`-prefixed or pre-release tag does not start it. It checks that the tag is on `main` and equals `<Version>`, runs the check, packs, and pushes the `.nupkg` and `.snupkg` to nuget.org.

GitHub setup: a `nuget` environment and a `NUGET_API_KEY` secret (scoped to `ThPostway.PostSdk`). Limit the environment's deployment tags to `[0-9]*.[0-9]*.[0-9]*`. Switch to NuGet trusted publishing once the package exists.

Never move or reuse a published tag. If `Publish` fails for a transient reason, re-run it (the push uses `--skip-duplicate`); otherwise fix forward with a hotfix and the next patch version.
