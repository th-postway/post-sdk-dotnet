# Security policy

## Supported versions

Only the latest major release of `ThPostway.PostSdk` receives security fixes.

## Reporting a vulnerability

Please do **not** open a public issue for security problems.

Use GitHub's private vulnerability reporting on this repository: open the **Security** tab and choose **Report a vulnerability**. Include the SDK version, a description of the issue, and steps to reproduce it.

You will receive an acknowledgement within three business days. Confirmed issues are fixed in the next patch release and credited in the changelog unless you prefer otherwise.

## What the SDK guarantees

- `BaseUrl` must be `https://`; plain `http://` is accepted only for loopback hosts.
- Header values (`AccessToken`, `TokenType`, `UserAgent`) are validated at construction so they cannot inject headers.
- Caller-supplied path parameters are percent-encoded and may not be empty, `.` or `..`.
- Redirects are never followed, and no request is ever retried.
- Exception messages and `Url` properties never contain tokens, tracking numbers or other path parameters, and response bodies are kept out of messages, `ToString()` and JSON serialization.
- The SDK has no runtime dependencies, does not log, and does not read environment variables.

If any of these do not hold, that is a vulnerability; please report it.
