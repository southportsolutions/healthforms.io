# HealthForms.io API client for .NET

[![NuGet](https://img.shields.io/nuget/v/HealthForms.Api)](https://www.nuget.org/packages/HealthForms.Api/)
[![NuGet](https://img.shields.io/nuget/v/HealthForms.Api.Core)](https://www.nuget.org/packages/HealthForms.Api.Core/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

Add participants to a [HealthForms.io](https://healthforms.io) session from your own system, assign them forms, and get a webhook when they submit.

The client targets .NET Standard 2.0, so it runs on .NET Framework 4.6.1 and later, .NET Core 2.0 and later, and .NET 5 and later. If you are not on .NET, the [authentication](#authentication) and [calling the API without the .NET client](#calling-the-api-without-the-net-client) sections give the raw HTTP, and every resource section names the route the client calls. The REST contract behind the client is in the [Swagger documentation](https://app.healthforms.io/dev/api/swagger/index.html).

## Quickstart

1. Install the package:

   ```bash
   dotnet add package HealthForms.Api
   ```

2. Add your OAuth client credentials to `appsettings.json`. Southport Solutions issues `CLIENT_ID` and `CLIENT_SECRET` when your app is registered. `REDIRECT_URL` is the callback in your app that receives the authorization code. A HealthForms.io engineer must register it against your client before the flow works, so send it along with your registration request. To change it later, or add one for a test environment, contact HealthForms.io again.

   ```json
   {
       "HealthForms": {
           "ClientId": "CLIENT_ID",
           "ClientSecret": "CLIENT_SECRET",
           "RedirectUrl": "REDIRECT_URL"
       }
   }
   ```

3. Register the client:

   ```csharp
   builder.Services.AddHealthForms(builder.Configuration.GetSection(HealthFormsApiOptions.Key));
   ```

4. Inject `IHealthFormsApiHttpClient` and list the members of a session. `TENANT_TOKEN` and `TENANT_ID` come from the [authentication](#authentication) flow, one pair per customer. `SESSION_ID` is any session ID from that tenant.

   ```csharp
   var response = await api.GetSessionMembers(TENANT_TOKEN, TENANT_ID, SESSION_ID);

   if (!response.IsSuccess)
   {
       throw new Exception($"{response.StatusCode}: {response.ErrorMessage}");
   }

   foreach (var member in response.Data.Data)
   {
       Console.WriteLine($"{member.FirstName} {member.LastName} {member.Status}");
   }
   ```

   The API returns JSON like this, which the client deserializes into `PagedResponse<List<SessionMemberResponse>>`:

   <!-- NEEDS: replace with a real response from the dev API. Shape below is built from the model and a two-member session. -->

   ```json
   {
     "data": [
       {
         "id": "a8Kd2mQz4p",
         "sessionId": "s7Hq3nRw9x",
         "externalAttendeeId": "camper-1042",
         "externalMemberId": null,
         "firstName": "Maya",
         "lastName": "Ortiz",
         "invitationSendOn": "2026-05-01T00:00:00Z",
         "invitationSentOn": "2026-05-01T09:00:12Z",
         "invitationAccepted": true,
         "status": "WaitingForForms",
         "statusDescription": "Waiting for forms",
         "isComplete": false,
         "tags": ["campers"],
         "formPacketIds": ["p2Lm8vBn6c"],
         "forms": [
           { "id": "f4Tx9yWq1e", "name": "Health History", "status": "NotStarted", "statusDescription": "Not started", "isRequired": true, "isComplete": false }
         ]
       }
     ],
     "nextUri": null,
     "responseCount": 1,
     "pageSize": 100,
     "totalItems": 1
   }
   ```

## Authentication

HealthForms.io uses OAuth 2.0 authorization code flow with a refresh token. Each customer authorizes your app once. You store the refresh token, called the tenant token in this library, and use it to get a bearer token for every API call. The steps below show the raw HTTP first and the .NET client call that performs it.

The authorization server is at `https://app.healthforms.io/account/`. Its discovery document is at [app.healthforms.io/account/.well-known/openid-configuration](https://app.healthforms.io/account/.well-known/openid-configuration), so any OpenID Connect library can drive this flow from `CLIENT_ID`, `CLIENT_SECRET`, and `REDIRECT_URL`.

| Setting | Value |
|---------|-------|
| Authorization endpoint | `https://app.healthforms.io/account/connect/authorize` |
| Token endpoint | `https://app.healthforms.io/account/connect/token` |
| Grant types | `authorization_code`, `refresh_token` |
| Client authentication | `client_id` and `client_secret` in the POST body |
| PKCE | Accepted with `S256`. The .NET client always sends it. |
| Scopes | `openid profile offline_access hf_public_all customer:TENANT_ID` |
| Authorization code lifetime | 5 minutes |
| Access token lifetime | 60 minutes |
| Refresh token | Reused on every refresh, not rotated. Expires after 10 years without use. |

### 1. Get the tenant ID

Ask the customer for their tenant ID. It is shown at [app.healthforms.io/manage/billing](https://app.healthforms.io/manage/billing). It is a 10-character string and it appears in every API route.

### 2. Send the customer to the authorization endpoint

Build the URL with the customer's tenant ID inside the `customer:` scope, then redirect the browser to it. The customer signs in to HealthForms.io and authorizes your app.

```
https://app.healthforms.io/account/connect/authorize
  ?client_id=CLIENT_ID
  &response_type=code
  &scope=openid%20profile%20offline_access%20hf_public_all%20customer%3ATENANT_ID
  &redirect_uri=REDIRECT_URL
  &state=STATE
  &code_challenge=CODE_CHALLENGE
  &code_challenge_method=S256
```

`redirect_uri` must match a URL registered for your client by HealthForms.io, character for character. An unregistered value stops the flow at the authorization endpoint with an error page instead of a redirect. `STATE` is a random value you check on the callback. `CODE_CHALLENGE` is the base64url-encoded SHA-256 of a random `CODE_VERIFIER` of 43 to 128 characters. Keep `CODE_VERIFIER` and `STATE` where your callback can read them, such as server-side session state. If your OAuth library does not support PKCE, omit the two `code_challenge` parameters.

The .NET client builds this URL and the verifier for you:

```csharp
var redirect = api.GetRedirectUrl(TENANT_ID);
HttpContext.Session.SetString("codeVerifier", redirect.CodeVerifier);
return Redirect(redirect.Uri);
```

### 3. Exchange the code for tokens

HealthForms.io redirects to `REDIRECT_URL?code=CODE&state=STATE&scope=...`. Check `state`, then POST the code to the token endpoint within five minutes.

```bash
curl https://app.healthforms.io/account/connect/token \
  -d grant_type=authorization_code \
  -d client_id=CLIENT_ID \
  -d client_secret=CLIENT_SECRET \
  -d code=CODE \
  -d redirect_uri=REDIRECT_URL \
  -d code_verifier=CODE_VERIFIER
```

<!-- NEEDS: a real token response. Shape below is a standard OAuth 2.0 token response; token values are placeholders. -->

```json
{
  "id_token": "eyJ...",
  "access_token": "eyJ...",
  "expires_in": 3600,
  "token_type": "Bearer",
  "refresh_token": "8A9F...",
  "scope": "openid profile offline_access hf_public_all customer:TENANT_ID"
}
```

Store `refresh_token` together with the tenant ID. The tenant ID is also in the `scope` value after `customer:`, which lets a single callback endpoint serve every customer.

```csharp
var tenantToken = await api.GetTenantToken(code, HttpContext.Session.GetString("codeVerifier"));
```

`GetTenantToken` returns only the refresh token. Call `ClaimCode` instead to get the full token response, and `GetTenantIdFromScope(response.Scope)` to read the tenant ID out of it.

### 4. Get a bearer token for each API call

Exchange the refresh token for an access token, then send the access token as a bearer token. Cache the access token and reuse it until it expires.

```bash
curl https://app.healthforms.io/account/connect/token \
  -d grant_type=refresh_token \
  -d client_id=CLIENT_ID \
  -d client_secret=CLIENT_SECRET \
  -d refresh_token=TENANT_TOKEN
```

The response has the same shape as step 3. `refresh_token` is the same value you sent, so there is nothing new to store.

```bash
curl https://app.healthforms.io/api/v1/TENANT_ID/sessions?startDate=2026-01-01 \
  -H "Authorization: Bearer ACCESS_TOKEN"
```

The .NET client does this inside every API method. It caches the access token in memory per tenant token and refreshes it five seconds before expiry. `GetAccessToken(tenantToken)` exposes the same cache if you need the bearer token for a call the client does not cover.

### Authentication failures

| Where | Response | Cause | Fix |
|-------|----------|-------|-----|
| Token endpoint | `400` `{"error":"invalid_grant"}` | The code expired, was already used, or `code_verifier` does not match. On refresh, the customer revoked your app. | Send the customer through step 2 again. |
| Token endpoint | `400` `{"error":"invalid_client"}` | `client_id` or `client_secret` is wrong. | Check your credentials. |
| API | `401` | The bearer token is missing, expired, or not for this API. | Refresh it. |
| API | `403` with code 4003 | The token was issued for a different tenant than the one in the URL. | Use the tenant ID from the token's `scope`. |

The .NET client throws `HealthFormsAuthException` for any token endpoint failure. The exception's `Response` property carries the raw token response.

## Calling the API without the .NET client

Every route is `https://app.healthforms.io/api/v1/TENANT_ID/...` and needs two headers:

```
Authorization: Bearer ACCESS_TOKEN
Content-Type: application/json
```

Bodies and responses are JSON with camelCase property names. Enums are strings. Dates are ISO 8601 in UTC. The [endpoint reference](#endpoint-reference) at the end of this README maps every client method to its route, and the [Swagger documentation](https://app.healthforms.io/dev/api/swagger/index.html) has the full request and response schemas. The `HealthForms.Api.Core` package holds the same models as C# classes, which is useful for a webhook receiver written in .NET even when the caller is not.

## Responses

Every method returns `HealthFormsApiResponse<T>`, or `HealthFormsApiResponse` for calls with no body. Check `IsSuccess` before reading `Data`.

| Property | Description |
|----------|-------------|
| `Data` | The deserialized body. Null when the call failed or a single-item lookup returned 404. |
| `StatusCode` | The HTTP status. `-1` when the client could not deserialize the body. |
| `IsSuccess` | `true` for any 2xx status. |
| `ErrorMessage` | The API's error message, prefixed with its four-digit code, or a client-side message. |
| `ErrorCode` | The four-digit code parsed from `ErrorMessage`, or null. |
| `Error` | The parsed error body with `Message` and `ValidationErrors`. |

Every method also accepts a `CancellationToken` as its last argument.

## Sessions

`GetSessions` lists sessions that start on or after a date, 50 per page. `GetSession` returns one session with its forms. `GetSessionSelectList` returns only ID, name, and dates, for building a dropdown.

```csharp
var sessions = await api.GetSessions(TENANT_TOKEN, TENANT_ID, new DateTime(2026, 1, 1));

while (sessions.IsSuccess && sessions.Data.NextUri != null)
{
    sessions = await api.GetSessions(TENANT_TOKEN, sessions.Data.NextUri);
}

var session = await api.GetSession(TENANT_TOKEN, TENANT_ID, SESSION_ID);
var select = await api.GetSessionSelectList(TENANT_TOKEN, TENANT_ID, new DateTime(2026, 1, 1));
```

Each `SessionFormResponse` in `session.Data.Forms` carries the session form ID. Form packets and member form assignments take that ID, not the form type ID.

## Form packets

A form packet groups session forms so you can assign them to a member in one step. Adding a form to a packet later also adds it to every member who holds the packet. A packet with tags is assigned automatically to members who share a tag. See [tags](#tags).

```csharp
var packets = await api.GetFormPackets(TENANT_TOKEN, TENANT_ID, SESSION_ID);

var created = await api.CreateFormPacket(TENANT_TOKEN, TENANT_ID, SESSION_ID, new CreateFormPacketRequest
{
    Name = "Camper Packet",
    SessionFormIds = ["f4Tx9yWq1e", "f8Ra2kPd5m"],
    Tags = ["campers"]
});

var updated = await api.UpdateFormPacket(TENANT_TOKEN, TENANT_ID, SESSION_ID, new UpdateFormPacketRequest
{
    Id = created.Data.Id,
    Name = "Camper Packet 2026"
});

await api.AddFormPacketForm(TENANT_TOKEN, TENANT_ID, SESSION_ID, created.Data.Id,
    new AddFormPacketFormRequest { SessionFormId = "f1Nz7cVb3h" });

await api.DeleteFormPacketForm(TENANT_TOKEN, TENANT_ID, SESSION_ID, created.Data.Id, "f1Nz7cVb3h", removeAssignedForms: false);
await api.DeleteFormPacket(TENANT_TOKEN, TENANT_ID, SESSION_ID, created.Data.Id, removeAssignedForms: false);
```

`UpdateFormPacket` leaves `Name`, `Description`, and `Tags` unchanged when they are blank or null. It does not change the packet's forms.

`removeAssignedForms` decides what happens to forms members already received through the packet. Leave it `false` to let members keep them. Pass `true` to remove them from every member as well.

## Session members

### Look up members

`GetSessionMembers` returns 100 members per page. The three single-member lookups return `Data = null` with status 404 when nothing matches.

```csharp
var members = await api.GetSessionMembers(TENANT_TOKEN, TENANT_ID, SESSION_ID);

var byId = await api.GetSessionMember(TENANT_TOKEN, TENANT_ID, SESSION_ID, "a8Kd2mQz4p");
var byMemberId = await api.GetSessionMemberByExternalId(TENANT_TOKEN, TENANT_ID, SESSION_ID, "reg-2026-0417");
var byAttendeeId = await api.GetSessionMemberByExternalAttendeeId(TENANT_TOKEN, TENANT_ID, SESSION_ID, "camper-1042");
```

To find out whether a person you are about to add already exists, use `SearchSessionMember`. It takes both external IDs and reports which one matched.

```csharp
var search = await api.SearchSessionMember(TENANT_TOKEN, TENANT_ID, SESSION_ID, new SessionMemberSearchRequest
{
    ExternalAttendeeId = "camper-1042",
    ExternalMemberId = "reg-2026-0417"
});
```

| `Result` | Meaning |
|----------|---------|
| `None` | Neither ID matched. |
| `Match` | Both IDs point to the same member, in `Member`. |
| `MatchExternalMemberId` | Only the member ID matched, in `Member`. |
| `MatchExternalAttendeeId` | Only the attendee ID matched, in `Attendee`. |
| `ExternalIdConflict` | The IDs matched two different members. Resolve before adding. |

### Add members

`ExternalAttendeeId` identifies the person across your whole tenant. `ExternalMemberId` identifies this person's registration in this session. Set both when you have them so later lookups and updates can find the member.

```csharp
var added = await api.AddSessionMember(TENANT_TOKEN, TENANT_ID, SESSION_ID, new AddSessionMemberRequest
{
    FirstName = "Maya",
    LastName = "Ortiz",
    Email = "parent@example.com",
    Phone = "555-555-0142",
    Group = "Cabin 4",
    ExternalAttendeeId = "camper-1042",
    ExternalMemberId = "reg-2026-0417",
    SendInvitationOn = new DateTime(2026, 5, 1),
    FormPacketIds = ["p2Lm8vBn6c"],
    Tags = ["campers"]
});
```

To add up to 1,000 members in one call, pass a list to `AddSessionMembers`. The API returns 202 with a job ID. Poll `GetAddSessionMembersStatus` until `PercentComplete` is 100, then read `Results` for any member with `Result == Skipped` and its `Message`.

```csharp
var job = await api.AddSessionMembers(TENANT_TOKEN, TENANT_ID, SESSION_ID, memberList);
var status = await api.GetAddSessionMembersStatus(TENANT_TOKEN, TENANT_ID, SESSION_ID, job.Data.Id);
```

### Update and delete members

`UpdateSessionMember` finds the member by `SessionMemberId`, then `ExternalAttendeeId`, then `ExternalMemberId`. Name, email, and phone change only until the member accepts the invitation. `Tags` defaults to an empty list, which clears the member's tags. Pass `null` to leave tags alone.

```csharp
var updated = await api.UpdateSessionMember(TENANT_TOKEN, TENANT_ID, SESSION_ID, new UpdateSessionMemberRequest
{
    SessionMemberId = "a8Kd2mQz4p",
    FirstName = "Maya",
    LastName = "Ortiz-Reyes",
    Email = "parent@example.com",
    Tags = null
});

await api.DeleteSessionMember(TENANT_TOKEN, TENANT_ID, SESSION_ID, "a8Kd2mQz4p");
await api.DeleteSessionMemberByExternalId(TENANT_TOKEN, TENANT_ID, SESSION_ID, "reg-2026-0417");
await api.DeleteSessionMemberByExternalAttendeeId(TENANT_TOKEN, TENANT_ID, SESSION_ID, "camper-1042");
```

### Assign forms and packets to a member

Assigning a form or packet the member already holds succeeds without change.

```csharp
await api.AddSessionMemberForm(TENANT_TOKEN, TENANT_ID, SESSION_ID, "a8Kd2mQz4p",
    new AddSessionMemberFormRequest { SessionFormId = "f4Tx9yWq1e" });

await api.DeleteSessionMemberForm(TENANT_TOKEN, TENANT_ID, SESSION_ID, "a8Kd2mQz4p", "f4Tx9yWq1e");

await api.AddSessionMemberFormPacket(TENANT_TOKEN, TENANT_ID, SESSION_ID, "a8Kd2mQz4p",
    new AddSessionMemberFormPacketRequest { FormPacketId = "p2Lm8vBn6c" });

await api.DeleteSessionMemberFormPacket(TENANT_TOKEN, TENANT_ID, SESSION_ID, "a8Kd2mQz4p", "p2Lm8vBn6c", removeAssignedForms: false);
```

## Tags

Member tags decide which forms and packets a member receives. A member gets every session form and every tagged packet that shares at least one tag with them, compared without regard to case. A form with no tags goes to every member. A packet with no tags is never assigned by tag. When a member's tags change, their forms are recomputed, and forms they have already started stay.

Session form and packet tags are the values member tags are matched against. Session tags are for organizing sessions and do not affect assignment.

A tag is at most 100 characters and cannot contain a comma. All tags on one entity together are at most 1,000 characters. Anything else fails validation with a `ValidationErrors` entry on `Tags`.

## Users

`AddUser` adds a staff user to the customer's HealthForms.io Manager account, or updates the user that already has that email address. On an existing user it updates the names, restores revoked access, and adds any missing roles. It never removes a role. New users receive an invitation email.

Only three roles can be granted through the API, each scoped to one session. Set `GroupId` to limit a role to one group in that session.

| Role | Grants |
|------|--------|
| `ParticipantViewer` | See participants and their status. |
| `ParticipantFormViewer` | Also open submitted forms. |
| `ParticipantFormReviewer` | Also approve or reject submissions. |

```csharp
var user = await api.AddUser(TENANT_TOKEN, TENANT_ID, new AddUserRequest
{
    FirstName = "Jordan",
    LastName = "Lee",
    EmailAddress = "nurse@example.com",
    Roles =
    [
        new AddUserRoleRequest { Role = UserRole.ParticipantFormReviewer, SessionId = SESSION_ID },
        new AddUserRoleRequest { Role = UserRole.ParticipantViewer, SessionId = SESSION_ID, GroupId = "g5Wq1zXc8v" }
    ]
});
```

## Webhooks

A subscription registers one HTTPS endpoint for one event type.

```csharp
var subscriptions = await api.GetWebhookSubscriptions(TENANT_TOKEN, TENANT_ID);

var subscription = await api.AddWebhookSubscription(TENANT_TOKEN, TENANT_ID, new WebhookSubscriptionRequest
{
    Type = WebhookType.SessionMemberFormUpdated,
    EndpointUrl = "https://example.com/healthforms/member-form"
});

await api.DeleteWebhookSubscription(TENANT_TOKEN, TENANT_ID, subscription.Data.Id);
```

HealthForms.io POSTs a JSON body to the endpoint. Deserialize it with the type for the subscribed event:

| `WebhookType` | Deserialize as | `Data` |
|---------------|----------------|--------|
| `SessionAdded`, `SessionUpdated` | `WebhookSession` | `SessionResponse` |
| `SessionMemberAdded`, `SessionMemberUpdated` | `WebhookSessionMember` | `SessionMemberResponse` |
| `SessionMemberFormAdded`, `SessionMemberFormUpdated` | `WebhookSessionMemberForm` | `SessionMemberFormResponse` |
| `SessionRemoved`, `SessionMemberRemoved`, `SessionMemberFormRemoved` | `WebhookDataRemoved` | none, the `Id` is on the envelope |

Each body has an `EventId` that is unique per delivery. Store it and skip a body you have already processed.

```csharp
[HttpPost("healthforms/member-form")]
public IActionResult MemberForm([FromBody] WebhookSessionMemberForm payload)
{
    if (payload.Data.IsComplete)
    {
        // mark the form complete in your system
    }
    return Ok();
}
```

<!-- NEEDS: webhook retry schedule and whether deliveries carry a signature header. The dispatcher is not in this repository. -->

## Errors

Failed calls come back in the response object. The API's error body is:

```json
{
  "message": "4012 - A participant with the External Attendee ID camper-1042 already exists.",
  "validationErrors": [
    { "field": "Tags", "message": "Tag 'a,b' cannot contain a comma." }
  ],
  "type": "SouthportError"
}
```

`message` starts with a four-digit code, which the client exposes as `ErrorCode`. `validationErrors` is present only for validation failures. Codes you are likely to hit:

| Status | Code | Cause | Fix |
|--------|------|-------|-----|
| 400 | 900 | Model binding failed. A required field is missing or has the wrong type. | Read `Error.Message` for the field. |
| 400 | 3000 | A member failed validation. | Read `Error.ValidationErrors`. |
| 400 | 3001 | `AddSessionMembers` was called with more than 1,000 members. | Split the list. |
| 400 | 3002 | `UpdateSessionMember` matched no member by any of its three IDs. | Look the member up first. |
| 400 | 3003 | The member ID does not exist. | |
| 400 | 3004 | The member exists but not in this session. | Check `SessionId` on the member. |
| 400 | 3005 | `SearchSessionMember` was called with both IDs null. | |
| 400 | 3007 | A session form ID does not belong to this session. | Take IDs from `session.Data.Forms`. |
| 400 | 3008 | Form packet validation failed. | Read `Error.ValidationErrors`. |
| 400 | 3009 | User validation failed. | Read `Error.ValidationErrors`. |
| 400 | 3010 | A role's `SessionId` does not exist. | |
| 400 | 3011 | A role's `GroupId` is not in that session. | |
| 400 | 4000 | The session, packet, or other resource does not exist. Note the status is 400, not 404. | |
| 401 | 4004 | The bearer token was rejected. | Re-authorize the tenant. |
| 403 | 4003 | The tenant token does not grant access to `TENANT_ID`. | Use the tenant ID the token was issued for. |
| 400 | 4010 | The tenant has no member credits left. | The customer must add credits in HealthForms.io. |
| 400 | 4012 | Another member in the tenant has this `ExternalAttendeeId`. | Use `SearchSessionMember` before adding. |
| 400 | 4013, 4014 | Another member has this `ExternalMemberId`. | Same. |
| 400 | 4015 | The session is locked. | The customer removes the lock in HealthForms.io. |
| 413 | | The request body is too large. | Send fewer members per bulk call. |

Only the three single-member `Get` methods and `GetSession` return 404. They set `Data` to null and `ErrorMessage` to `Not Found` with no error body.

## Pagination

`GetSessions` and `GetSessionMembers` return `PagedResponse<T>`. Pass `Data.NextUri` to the overload that takes `(tenantToken, nextUri)` until it is null. `TotalItems` and `PageSize` are on every page.

<!-- NEEDS: rate limits. Nothing in the API source enforces one; confirm whether the gateway does. -->

## Endpoint reference

Routes are relative to `https://app.healthforms.io/api/`. Query parameters shown with an empty value are optional except `startDate` on the session list.

| Client method | HTTP | Route |
|---------------|------|-------|
| `GetSessions` | GET | `v1/{tenantId}/sessions?startDate=&page=` |
| `GetSession` | GET | `v1/{tenantId}/sessions/{sessionId}` |
| `GetSessionSelectList` | GET | `v1/{tenantId}/sessions/select?startDate=` |
| `GetFormPackets` | GET | `v1/{tenantId}/sessions/{sessionId}/form-packets` |
| `CreateFormPacket` | POST | `v1/{tenantId}/sessions/{sessionId}/form-packets` |
| `UpdateFormPacket` | PUT | `v1/{tenantId}/sessions/{sessionId}/form-packets` |
| `DeleteFormPacket` | DELETE | `v1/{tenantId}/sessions/{sessionId}/form-packets/{formPacketId}?removeAssignedForms=` |
| `AddFormPacketForm` | POST | `v1/{tenantId}/sessions/{sessionId}/form-packets/{formPacketId}/forms` |
| `DeleteFormPacketForm` | DELETE | `v1/{tenantId}/sessions/{sessionId}/form-packets/{formPacketId}/forms/{sessionFormId}?removeAssignedForms=` |
| `GetSessionMembers` | GET | `v1/{tenantId}/sessions/{sessionId}/members?page=` |
| `GetSessionMember` | GET | `v1/{tenantId}/sessions/{sessionId}/members/{memberId}` |
| `GetSessionMemberByExternalId` | GET | `v1/{tenantId}/sessions/{sessionId}/members/external/{externalMemberId}` |
| `GetSessionMemberByExternalAttendeeId` | GET | `v1/{tenantId}/sessions/{sessionId}/members/external-attendee/{externalAttendeeId}` |
| `SearchSessionMember` | POST | `v1/{tenantId}/sessions/{sessionId}/members/search` |
| `AddSessionMember` | POST | `v1/{tenantId}/sessions/{sessionId}/members` |
| `AddSessionMembers` | POST | `v1/{tenantId}/sessions/{sessionId}/members/bulk` |
| `GetAddSessionMembersStatus` | GET | `v1/{tenantId}/sessions/{sessionId}/members/bulk/{bulkId}` |
| `UpdateSessionMember` | PUT | `v1/{tenantId}/sessions/{sessionId}/members` |
| `DeleteSessionMember` | DELETE | `v1/{tenantId}/sessions/{sessionId}/members/{memberId}` |
| `DeleteSessionMemberByExternalId` | DELETE | `v1/{tenantId}/sessions/{sessionId}/members/external/{externalMemberId}` |
| `DeleteSessionMemberByExternalAttendeeId` | DELETE | `v1/{tenantId}/sessions/{sessionId}/members/external-attendee/{externalAttendeeId}` |
| `AddSessionMemberForm` | POST | `v1/{tenantId}/sessions/{sessionId}/members/{memberId}/forms` |
| `DeleteSessionMemberForm` | DELETE | `v1/{tenantId}/sessions/{sessionId}/members/{memberId}/forms/{sessionFormId}` |
| `AddSessionMemberFormPacket` | POST | `v1/{tenantId}/sessions/{sessionId}/members/{memberId}/form-packets` |
| `DeleteSessionMemberFormPacket` | DELETE | `v1/{tenantId}/sessions/{sessionId}/members/{memberId}/form-packets/{formPacketId}?removeAssignedForms=` |
| `AddUser` | POST | `v1/{tenantId}/users` |
| `GetWebhookSubscriptions` | GET | `v1/{tenantId}/webhooks` |
| `AddWebhookSubscription` | POST | `v1/{tenantId}/webhooks` |
| `DeleteWebhookSubscription` | DELETE | `v1/{tenantId}/webhooks/{webhookId}` |

## .NET Framework without dependency injection

`HealthFormsApiHttpClientDisposable` owns its `HttpClient`. Construct it once and reuse it.

```csharp
var options = new HealthFormsApiOptions
{
    ClientId = "CLIENT_ID",
    ClientSecret = "CLIENT_SECRET",
    RedirectUrl = "REDIRECT_URL"
};

using var client = new HealthFormsApiHttpClientDisposable(new HttpClient(), options);
var sessions = await client.GetSessions(TENANT_TOKEN, TENANT_ID, DateTime.Today);
```

The DI registration also adds a Polly retry policy with jittered exponential backoff. The disposable client does not, so add your own if you need retries.

## Packages

| Package | Contents |
|---------|----------|
| [HealthForms.Api](https://www.nuget.org/packages/HealthForms.Api/) | Client, OAuth, DI extension, retry policy. Depends on Core. |
| [HealthForms.Api.Core](https://www.nuget.org/packages/HealthForms.Api.Core/) | Request, response, and webhook models only. Reference this alone in a webhook receiver that does not call the API. |

| Package | Build |
|---------|-------|
| HealthForms.Api | [![Library - Build and Deploy](https://github.com/southportsolutions/healthforms.io/actions/workflows/main.yml/badge.svg)](https://github.com/southportsolutions/healthforms.io/actions/workflows/main.yml) |
| HealthForms.Api.Core | [![Core - Build and Deploy](https://github.com/southportsolutions/healthforms.io/actions/workflows/main-core.yml/badge.svg)](https://github.com/southportsolutions/healthforms.io/actions/workflows/main-core.yml) |

## Samples and support

- `samples/HealthForms.Api.Sample/` is an ASP.NET Core 8 app that walks through authorization and the member calls.
- `samples/HealthForms.Api.Sample.FullFramework/` is a .NET Framework 4.6.2 WebForms app using the disposable client.
- Report bugs in this library at [github.com/southportsolutions/healthforms.io/issues](https://github.com/southportsolutions/healthforms.io/issues).

Licensed under the [MIT License](https://opensource.org/licenses/MIT).
