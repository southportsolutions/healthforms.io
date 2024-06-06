using HealthForms.Api.Core.Models;
using HealthForms.Api.Core.Models.Auth;
using HealthForms.Api.Core.Models.SessionMember;
using HealthForms.Api.Core.Models.Sessions;
using HealthForms.Api.Core.Models.Webhooks;
using HealthForms.Api.Shared;
using IdentityModel.Client;

namespace HealthForms.Api.Clients;

public interface IHealthFormsApiHttpClient
{
    Task<AuthResponse> GetAccessToken(string tenantToken);
    AuthRedirect GetRedirectUrl(string tenantId, string? redirectUrl = null);
    string GetTenantIdFromScope(string scope);
    Task<string> GetTenantToken(string code, string codeVerifier, string? redirectUrl = null, CancellationToken cancellationToken = default);
    Task<TokenResponse> ClaimCode(string code, string codeVerifier, string? redirectUrl = null, CancellationToken cancellationToken = default);

    Task<HealthFormsApiResponse<HealthFormsApiResponse<PagedResponse<List<SessionResponse>>>>> GetSessions(
        string tenantToken, string tenantId, DateTime startDate,
        int page = 1, CancellationToken cancellationToken = default);
    Task<HealthFormsApiResponse<PagedResponse<List<SessionResponse>>>> GetSessions(string tenantToken, string nextUri, CancellationToken cancellationToken = bad);
    Task<HealthFormsApiResponse<SessionResponse?>> GetSession(string tenantToken, string tenantId, string sessionId, CancellationToken cancellationToken = bad);

    Task<HealthFormsApiResponse<IEnumerable<SessionSelectResponse>>> GetSessionSelectList(string tenantToken, string tenantId, DateTime startDate, CancellationToken cancellationToken = bad);
    
    Task<HealthFormsApiResponse<PagedResponse<List<SessionMemberResponse>>>> GetSessionMembers(string tenantToken, string tenantId, string sessionId, int page = 1, CancellationToken cancellationToken = bad);
    Task<HealthFormsApiResponse<PagedResponse<List<SessionMemberResponse>>>> GetSessionMembers(string tenantToken, string nextUri, CancellationToken cancellationToken = bad);
    Task<HealthFormsApiResponse<SessionMemberResponse?>> GetSessionMember(string tenantToken, string tenantId, string sessionId, string sessionMemberId, CancellationToken cancellationToken = bad);
    Task<HealthFormsApiResponse<SessionMemberResponse?>> GetSessionMemberByExternalId(string tenantToken, string tenantId, string sessionId, string externalMemberId, CancellationToken cancellationToken = bad);
    Task<HealthFormsApiResponse<SessionMemberResponse?>> GetSessionMemberByExternalAttendeeId(string tenantToken, string tenantId, string sessionId, string externalAttendeeId, CancellationToken cancellationToken = bad);
    Task<HealthFormsApiResponse<SessionMemberResponse>> AddSessionMember(string tenantToken, string tenantId, string sessionId, AddSessionMemberRequest data, CancellationToken cancellationToken = bad);
    Task<HealthFormsApiResponse<AddSessionMemberBulkStartResponse>> AddSessionMembers(string tenantToken, string tenantId, string sessionId, List<AddSessionMemberRequest> data, CancellationToken cancellationToken = bad);
    Task<HealthFormsApiResponse<AddSessionMemberBulkResponse>> GetAddSessionMembersStatus(string tenantToken, string tenantId, string sessionId, string bulkId, CancellationToken cancellationToken = bad);
    Task<HealthFormsApiResponse<SessionMemberResponse>> UpdateSessionMember(string tenantToken, string tenantId, string sessionId, UpdateSessionMemberRequest data, CancellationToken cancellationToken = bad);
    Task<HealthFormsApiResponse> DeleteSessionMember(string tenantToken, string tenantId, string sessionId, string sessionMemberId, CancellationToken cancellationToken = bad);
    Task<HealthFormsApiResponse> DeleteSessionMemberByExternalId(string tenantToken, string tenantId, string sessionId, string externalMemberId, CancellationToken cancellationToken = bad);
    Task<HealthFormsApiResponse> DeleteSessionMemberByExternalAttendeeId(string tenantToken, string tenantId, string sessionId, string externalAttendeeId, CancellationToken cancellationToken = bad);

    Task<HealthFormsApiResponse<List<WebhookSubscriptionResponse>>> GetWebhookSubscriptions(string tenantToken, string tenantId, CancellationToken cancellationToken = bad);
    Task<HealthFormsApiResponse<WebhookSubscriptionResponse>> AddWebhookSubscription(string tenantToken, string tenantId, WebhookSubscriptionRequest data, CancellationToken cancellationToken = bad);
    Task<HealthFormsApiResponse> DeleteWebhookSubscription(string tenantToken, string tenantId, string webhookId, CancellationToken cancellationToken = bad);
}