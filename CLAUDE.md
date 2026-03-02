# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

This is the **HealthForms.io Public API .NET Client Library**, published as NuGet packages (`HealthForms.Api` and `HealthForms.Api.Core`). It provides a typed HTTP client for integrating with the HealthForms.io REST API, handling OAuth authentication, session/member management, and webhook subscriptions.

## Build Commands

```bash
# Build the entire solution
dotnet build HealthForms.Api.Library.sln

# Build individual projects
dotnet build src/HealthForms.Api.Core/HealthForms.Api.Core.csproj
dotnet build src/HealthForms.Api/HealthForms.Api.csproj

# Run tests (requires environment variables, see below)
dotnet test src/HealthForms.Api.Tests/HealthForms.Api.Tests.csproj

# Run a single test
dotnet test src/HealthForms.Api.Tests/HealthForms.Api.Tests.csproj --filter "FullyQualifiedName~MethodName"

# Pack for NuGet
dotnet pack src/HealthForms.Api.Core/HealthForms.Api.Core.csproj --configuration Release
dotnet pack src/HealthForms.Api/HealthForms.Api.csproj --configuration Release
```

## Test Configuration

Tests are **integration tests** that call the live HealthForms.io dev API. They require these environment variables (or an `appsettings.json` in the test project root):

- `HFPCLIENTID` - OAuth client ID
- `HFPCLIENTSECRET` - OAuth client secret
- `HFTENANTTOKEN` - Tenant refresh token
- `HFTENANTID` - Tenant ID
- `HFSESSIONID` - Session ID for test operations
- `HDREDIRECTURL` - OAuth redirect URL

Tests use **xUnit** with **AutoFixture + AutoMoq** for test data generation and **Moq** for mocking. The test base class `UnitTestBase<T>` provides the AutoFixture setup. Test options (`HealthFormsApiTestOptions` in `src/HealthForms.Api.Tests/Options/`) override the API host to point to the dev environment (`{HostAddress}dev/api/` and `{HostAddress}dev/account/`).

## Architecture

### Two-Package Structure

- **`HealthForms.Api.Core`** (netstandard2.0) — Models only, no HTTP dependencies. Contains all request/response DTOs organized by domain: `Models/Sessions/`, `Models/SessionMember/`, `Models/Webhooks/`, `Models/Auth/`, `Models/Errors/`, `Models/FormType/`. Depends only on `System.Text.Json`. This package is referenced by `HealthForms.Api` as a NuGet package dependency (not a project reference), so changes to Core models require publishing a new Core version first.

- **`HealthForms.Api`** (netstandard2.0) — HTTP client, OAuth logic, DI extensions. Key types:
  - `IHealthFormsApiHttpClient` / `HealthFormsApiHttpClient` — Main client interface and implementation
  - `HealthFormsApiHttpClientDisposable` — Subclass for .NET Framework (non-DI) usage, manages HttpClient disposal
  - `HealthFormsApiDependencyInjectionExtension.AddHealthForms()` — Registers the typed HttpClient with Polly retry policy
  - `HealthFormsApiOptions` — Configuration POCO (config key: `"HealthForms"`)
  - `HealthFormsApiResponse<T>` — Standard wrapper for all API responses with `Data`, `StatusCode`, `IsSuccess`, `Error`
  - `TokenCache` — Static in-memory cache for OAuth access tokens keyed by tenant refresh token

### API Client Pattern

All API methods follow the same pattern: accept `tenantToken` (refresh token) + `tenantId` + resource identifiers, internally call `GetAccessToken()` to exchange/cache the access token, then make the HTTP request. Responses are wrapped in `HealthFormsApiResponse<T>` — errors are returned in the response object (not thrown), except for auth failures which throw `HealthFormsAuthException`.

The API routes are versioned as `v1/{tenantId}/...` and the base URL is derived from `HealthFormsApiOptions.HostAddressApi`.

### JSON Serialization

Uses `System.Text.Json` with `camelCase` naming policy and `JsonStringEnumConverter`. This applies across both Core models and API client deserialization.

## Versioning and Releases

Package versions are set in each `.csproj` file's `<Version>` element. When releasing:

1. **Core changes**: Update `HealthForms.Api.Core.csproj` version, tag with `release-core@X.Y.Z`, then update the `HealthForms.Api.Core` PackageReference in `HealthForms.Api.csproj`
2. **API changes**: Update `HealthForms.Api.csproj` version, tag with `release@X.Y.Z`

GitHub Actions workflows in `.github/workflows/`:
- `main.yml` — Builds and packs `HealthForms.Api`; publishes to NuGet on `release@*` tags
- `main-core.yml` — Builds and packs `HealthForms.Api.Core`; publishes on `release-core@*` tags

## Samples

- `samples/HealthForms.Api.Sample/` — ASP.NET Core 8 sample app
- `samples/HealthForms.Api.Sample.FullFramework/` — .NET Framework 4.6.2 WebForms sample using `HealthFormsApiHttpClientDisposable`
