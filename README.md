# [HealthForms.io](https://healthforms.io) Public API Library

[![NuGet](https://img.shields.io/nuget/v/HealthForms.Api)](https://www.nuget.org/packages/HealthForms.Api/)
[![NuGet](https://img.shields.io/nuget/v/HealthForms.Api.Core)](https://www.nuget.org/packages/HealthForms.Api.Core/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

A .NET client library for integrating with the [HealthForms.io](https://healthforms.io) API. Manage sessions, session members, and webhook subscriptions. Available on [NuGet](https://www.nuget.org/packages/HealthForms.Api/).

**Targets:** .NET Standard 2.0 (compatible with .NET Framework 4.6.1+, .NET Core 2.0+, .NET 5+)

## Build Status

| Package | Build |
|---------|-------|
| HealthForms.Api | [![Library - Build and Deploy](https://github.com/southportsolutions/healthforms.io/actions/workflows/main.yml/badge.svg)](https://github.com/southportsolutions/healthforms.io/actions/workflows/main.yml) |
| HealthForms.Api.Core | [![Core - Build and Deploy](https://github.com/southportsolutions/healthforms.io/actions/workflows/main-core.yml/badge.svg)](https://github.com/southportsolutions/healthforms.io/actions/workflows/main-core.yml) |

## Installation

```bash
dotnet add package HealthForms.Api
```

The `HealthForms.Api.Core` package (models only, no HTTP dependencies) is included automatically as a dependency.

## Quick Start (.NET Core / .NET 5+)

### 1. Add Configuration

Add the following to your `appsettings.json`:

```json
{
    "HealthForms": {
        "ClientId": "your-client-id",
        "ClientSecret": "your-client-secret",
        "RedirectUrl": "https://yourapp.com/callback"
    }
}
```

### 2. Register Services

```csharp
var healthFormsSection = builder.Configuration.GetSection(HealthFormsApiOptions.Key);
builder.Services.AddHealthForms(healthFormsSection);
```

This registers `IHealthFormsApiHttpClient` as a typed `HttpClient` with automatic retry policies (Polly with jittered exponential backoff).

### 3. Inject and Use

```csharp
public class SampleController : Controller
{
    private readonly IHealthFormsApiHttpClient _healthFormsApi;

    public SampleController(IHealthFormsApiHttpClient healthFormsApi)
    {
        _healthFormsApi = healthFormsApi;
    }

    public async Task<IActionResult> GetMembers(string tenantToken, string tenantId, string sessionId)
    {
        var response = await _healthFormsApi.GetSessionMembers(tenantToken, tenantId, sessionId);
        if (response.IsSuccess)
        {
            return Ok(response.Data);
        }
        return StatusCode(response.StatusCode, response.ErrorMessage);
    }
}
```

## Authentication & Authorization

The HealthForms API uses OAuth with PKCE to authenticate and authorize access. Each customer tenant must authorize your app through a 3-step flow:

**Step 1:** Obtain the Tenant ID from the customer. This is available at https://app.healthforms.io/manage/billing.

**Step 2:** Generate the authorization redirect URL and redirect the user:

```csharp
var authRedirect = healthFormsApi.GetRedirectUrl(tenantId);

// Store authRedirect.CodeVerifier where your callback endpoint can access it (e.g., session state)
// Redirect the user to authRedirect.Uri
```

The user will be prompted to consent to your app accessing their data. After clicking Allow, HealthForms.io redirects back to your configured `RedirectUrl` with an authorization code.

**Step 3:** Exchange the authorization code for a tenant token:

```csharp
var tenantToken = await healthFormsApi.GetTenantToken(code, codeVerifier);

// Store tenantToken securely - this is the refresh token that grants ongoing access to this tenant
```

The tenant token is a refresh token. The library automatically handles exchanging it for short-lived access tokens and caching them.

## API Methods

All API methods return `HealthFormsApiResponse<T>` which wraps the response with `Data`, `StatusCode`, `IsSuccess`, `ErrorMessage`, and `Error` properties. Check `IsSuccess` before accessing `Data`.

### Sessions

```csharp
// List sessions with pagination
var sessions = await api.GetSessions(tenantToken, tenantId, startDate, page: 1);

// Follow pagination
if (sessions.Data.NextUri != null)
    var nextPage = await api.GetSessions(tenantToken, sessions.Data.NextUri);

// Get a single session
var session = await api.GetSession(tenantToken, tenantId, sessionId);

// Get a lightweight list for dropdowns/selects
var selectList = await api.GetSessionSelectList(tenantToken, tenantId, startDate);
```

### Session Members

```csharp
// List members with pagination
var members = await api.GetSessionMembers(tenantToken, tenantId, sessionId, page: 1);

// Get a single member (by ID, external ID, or external attendee ID)
var member = await api.GetSessionMember(tenantToken, tenantId, sessionId, memberId);
var member = await api.GetSessionMemberByExternalId(tenantToken, tenantId, sessionId, externalId);
var member = await api.GetSessionMemberByExternalAttendeeId(tenantToken, tenantId, sessionId, externalAttendeeId);

// Search members
var results = await api.SearchSessionMember(tenantToken, tenantId, sessionId, new SessionMemberSearchRequest
{
    ExternalAttendeeId = "attendee-123",
    ExternalMemberId = "member-456"
});

// Add a member
var added = await api.AddSessionMember(tenantToken, tenantId, sessionId, new AddSessionMemberRequest
{
    FirstName = "John",
    LastName = "Doe",
    Email = "john@example.com",
    Phone = "555-555-5555",
    ExternalAttendeeId = "attendee-123",
    SendInvitationOn = DateTime.UtcNow.AddDays(1)
});

// Bulk add members (up to 1,000 per request)
var bulkStart = await api.AddSessionMembers(tenantToken, tenantId, sessionId, memberList);
// Poll for completion
var status = await api.GetAddSessionMembersStatus(tenantToken, tenantId, sessionId, bulkStart.Data.Id);

// Update a member
var updated = await api.UpdateSessionMember(tenantToken, tenantId, sessionId, updateRequest);

// Delete a member (by ID, external ID, or external attendee ID)
var deleted = await api.DeleteSessionMember(tenantToken, tenantId, sessionId, memberId);
var deleted = await api.DeleteSessionMemberByExternalId(tenantToken, tenantId, sessionId, externalId);
var deleted = await api.DeleteSessionMemberByExternalAttendeeId(tenantToken, tenantId, sessionId, externalAttendeeId);
```

### Webhooks

```csharp
// List webhook subscriptions
var webhooks = await api.GetWebhookSubscriptions(tenantToken, tenantId);

// Subscribe to events
var subscription = await api.AddWebhookSubscription(tenantToken, tenantId, new WebhookSubscriptionRequest
{
    EndpointUrl = "https://yourapp.com/webhook",
    Type = WebhookType.SessionMemberAdded
});

// Delete a subscription
await api.DeleteWebhookSubscription(tenantToken, tenantId, webhookId);
```

Available webhook types: `SessionAdded`, `SessionRemoved`, `SessionUpdated`, `SessionMemberAdded`, `SessionMemberRemoved`, `SessionMemberUpdated`.

## .NET Framework 4.6.2+

For projects not using dependency injection, use the `HealthFormsApiHttpClientDisposable` class which manages the `HttpClient` lifecycle:

```csharp
var options = new HealthFormsApiOptions
{
    ClientId = "your-client-id",
    ClientSecret = "your-client-secret",
    RedirectUrl = "https://yourapp.com/callback"
};

using var client = new HealthFormsApiHttpClientDisposable(new HttpClient(), options);

var sessions = await client.GetSessions(tenantToken, tenantId, DateTime.Today);
```

Follow the [Authentication & Authorization](#authentication--authorization) steps above to obtain the tenant token.

## Packages

| Package | Description |
|---------|-------------|
| [HealthForms.Api](https://www.nuget.org/packages/HealthForms.Api/) | Full client library with HTTP client, OAuth, DI extensions, and retry policies |
| [HealthForms.Api.Core](https://www.nuget.org/packages/HealthForms.Api.Core/) | Models and DTOs only (no HTTP dependencies) |

## License

This project is licensed under the [MIT License](https://opensource.org/licenses/MIT).
