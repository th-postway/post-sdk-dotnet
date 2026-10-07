# Changelog

All notable changes to this project are documented here. The format follows [Keep a Changelog](https://keepachangelog.com/en/1.1.0/). The major version tracks the target runtime (`8.x` → `net8.0`).

## [Unreleased]

### Added

- Runnable Quick start in `demo/` (`dotnet run --project demo`): read-only and sandbox by default, with an opt-in sandbox create + label (`POSTWAY_DEMO_CREATE=1`). Part of `Postway.sln`, so the check formats and builds it. Not packable.

### Changed

- Release tags are bare SemVer (`MAJOR.MINOR.PATCH`, no `v` prefix) and are cut through git-flow: CI runs on `develop`, `release/**` and `hotfix/**`, and Publish only accepts a tag on `main` that equals the package version. Dependabot targets `develop`; the duplicate `dotnet-build.yml` workflow is removed.

## [8.0.0] - 2026-10-07

Rewrite with the same surface, behaviour and security rules as the Node SDK (`@th-postway/post-sdk` 22.0.0). Published as `ThPostway.PostSdk`.

### Added

- `PostwayMerchantClient` with resources for auth, order shipments, shipment providers, Thailand postal areas, labels, public receipts and health, covering the full Merchant API method catalogue.
- `PostwayMerchantClientOptions` (`AccessToken`, `TokenType`, `Environment`, `BaseUrl`, `Timeout`, `HttpClient`, `UserAgent`) and per-call `RequestOptions` plus `CancellationToken` on every method.
- `MerchantEnvironment` (`Production`, `Sandbox`) and `MerchantBaseUrls`.
- Typed exceptions: `PostwayApiException`, `PostwayBusinessException`, `PostwayRequestException`, `PostwayConfigException`, all extending `PostwayException`.
- Wire enum constants (`LabelSize`, `OrderShipmentStatus`, ...) and the `FlashArticleCategory` enum.
- `FileHttpResponse.DecodeContent()` for base64 label and receipt files.
- `PostwaySdk.Version`.

### Removed

- The legacy `Post` / `IPost` API, `HttpClientService`, `JsonExtension`, `CONST.VALUE` and the `ViewModels` namespace. See "Mapping from the legacy `Post` / `IPost` API" in the README.

### Security

- `BaseUrl` must be `https://` (plain `http://` only for loopback hosts) with no credentials, query or fragment.
- `AccessToken`, `TokenType` and `UserAgent` are validated as safe header values at construction.
- Caller-supplied path parameters are percent-encoded and may not be empty, `.` or `..`.
- Redirects are refused.
- Exception `Url` and `Message` report route templates (`receipt/public/:token`) instead of parameter values; `PostwayApiException.Body` is excluded from messages and JSON serialization.
- `PostwayConfigException` messages never echo the offending input.

[Unreleased]: https://github.com/th-postway/post-sdk-dotnet/compare/8.0.0...HEAD
[8.0.0]: https://github.com/th-postway/post-sdk-dotnet/releases/tag/8.0.0
