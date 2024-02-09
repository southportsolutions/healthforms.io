using HealthForms.Api.Core.Models;
using HealthForms.Api.Core.Models.Auth;
using HealthForms.Api.Core.Models.SessionMember;
using HealthForms.Api.Core.Models.Sessions;
using HealthForms.Api.Core.Models.Webhooks;
using IdentityModel.Client;

namespace HealthForms.Api.Clients;

public interface IHealthFormsApiHttpClient
{
    Task<AuthResponse> GetAccessToken(string tenantToken);
    AuthRedirect GetRedirectUrl(string tenantId);
    string GetTenantIdFromScope(string scope);
    Task<string> GetTenantToken(string code, string codeVerifier, CancellationToken cancellationToken = default);
    Task<TokenResponse> ClaimCode(string code, string codeVerifier, CancellationToken cancellationToken = default);

    Task<PagedResponse<List<SessionResponse>>> GetSessions(string tenantToken, string tenantId, DateTime startDate, 
        int page = 1, CancellationToken cancellationToken = default);
    Task<PagedResponse<List<SessionResponse>>> GetSessions(string tenantToken, string nextUri, CancellationToken cancellationToken = default);
    Task<SessionResponse> GetSession(string tenantToken, string tenantId, string sessionId, CancellationToken cancellationToken = default);

    Task<IEnumerable<SessionSelectResponse>> GetSessionSelectList(string tenantToken, string tenantId, DateTime startDate, CancellationToken cancellationToken = default);
    
    Task<PagedResponse<List<SessionMemberResponse>>> GetSessionMembers(string tenantToken, string tenantId, string sessionId, int page = 1, CancellationToken cancellationToken = default);
    Task<PagedResponse<List<SessionMemberResponse>>> GetSessionMembers(string tenantToken, string nextUri, CancellationToken cancellationToken = default);
    Task<SessionMemberResponse> GetSessionMember(string tenantToken, string tenantId, string sessionId, string sessionMemberId, CancellationToken cancellationToken = default);
    Task<SessionMemberResponse> GetSessionMemberByExternalId(string tenantToken, string tenantId, string sessionId, string externalMemberId, CancellationToken cancellationToken = default);
    Task<SessionMemberResponse> GetSessionMemberByExternalAttendeeId(string tenantToken, string tenantId, string sessionId, string externalAttendeeId, CancellationToken cancellationToken = default);
    Task<AddSessionMemberResponse> AddSessionMember(string tenantToken, string tenantId, string sessionId, AddSessionMemberRequest data, CancellationToken cancellationToken = default);
    Task<AddSessionMemberBulkStartResponse> AddSessionMembers(string tenantToken, string tenantId, string sessionId, List<AddSessionMemberRequest> data, CancellationToken cancellationToken = default);
    Task<AddSessionMemberBulkResponse> GetAddSessionMembersStatus(string tenantToken, string tenantId, string sessionId, string bulkId, CancellationToken cancellationToken = default);
    Task<bool> DeleteSessionMember(string tenantToken, string tenantId, string sessionId, string sessionMemberId, CancellationToken cancellationToken = default);
    Task<bool> DeleteSessionMemberByExternalId(string tenantToken, string tenantId, string sessionId, string externalMemberId, CancellationToken cancellationToken = default);
    Task<bool> DeleteSessionMemberByExternalAttendeeId(string tenantToken, string tenantId, string sessionId, string externalAttendeeId, CancellationToken cancellationToken = default);

    Task<List<WebhookSubscriptionResponse>> GetWebhookSubscriptions(string tenantToken, string tenantId, CancellationToken cancellationToken = default);
    Task<WebhookSubscriptionResponse> AddWebhookSubscription(string tenantToken, string tenantId, WebhookSubscriptionRequest data, CancellationToken cancellationToken = default);
    Task<bool> DeleteWebhookSubscription(string tenantToken, string tenantId, string webhookId, CancellationToken cancellationToken = default);
}