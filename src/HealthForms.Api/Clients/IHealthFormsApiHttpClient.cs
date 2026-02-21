using HealthForms.Api.Core.Models;
using HealthForms.Api.Core.Models.Auth;
using HealthForms.Api.Core.Models.FormType;
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

    Task<HealthFormsApiResponse<PagedResponse<List<SessionResponse>>>> GetSessions(
        string tenantToken, string tenantId, DateTime startDate,
        int page = 1, CancellationToken cancellationToken = default);
    Task<HealthFormsApiResponse<PagedResponse<List<SessionResponse>>>> GetSessions(string tenantToken, string nextUri, CancellationToken cancellationToken = default);
    Task<HealthFormsApiResponse<SessionResponse?>> GetSession(string tenantToken, string tenantId, string sessionId, CancellationToken cancellationToken = default);

    Task<HealthFormsApiResponse<IEnumerable<SessionSelectResponse>>> GetSessionSelectList(string tenantToken, string tenantId, DateTime startDate, CancellationToken cancellationToken = default);
    Task<HealthFormsApiResponse<List<FormPacketResponse>>> GetFormPackets(string tenantToken, string tenantId, string sessionId, CancellationToken cancellationToken = default);
    Task<HealthFormsApiResponse<FormPacketResponse>> CreateFormPacket(string tenantToken, string tenantId, string sessionId, CreateFormPacketRequest data, CancellationToken cancellationToken = default);
    Task<HealthFormsApiResponse<FormPacketResponse>> UpdateFormPacket(string tenantToken, string tenantId, string sessionId, UpdateFormPacketRequest data, CancellationToken cancellationToken = default);

    Task<HealthFormsApiResponse<PagedResponse<List<SessionMemberResponse>>>> GetSessionMembers(string tenantToken, string tenantId, string sessionId, int page = 1, CancellationToken cancellationToken = default);
    Task<HealthFormsApiResponse<PagedResponse<List<SessionMemberResponse>>>> GetSessionMembers(string tenantToken, string nextUri, CancellationToken cancellationToken = default);
    Task<HealthFormsApiResponse<SessionMemberResponse?>> GetSessionMember(string tenantToken, string tenantId, string sessionId, string sessionMemberId, CancellationToken cancellationToken = default);
    Task<HealthFormsApiResponse<SessionMemberResponse?>> GetSessionMemberByExternalId(string tenantToken, string tenantId, string sessionId, string externalMemberId, CancellationToken cancellationToken = default);
    Task<HealthFormsApiResponse<SessionMemberResponse?>> GetSessionMemberByExternalAttendeeId(string tenantToken, string tenantId, string sessionId, string externalAttendeeId, CancellationToken cancellationToken = default);
    Task<HealthFormsApiResponse<SessionMemberSearchResponse>> SearchSessionMember(string tenantToken, string tenantId, string sessionId, SessionMemberSearchRequest request, CancellationToken cancellationToken = default);
    Task<HealthFormsApiResponse<SessionMemberResponse>> AddSessionMember(string tenantToken, string tenantId, string sessionId, AddSessionMemberRequest data, CancellationToken cancellationToken = default);
    Task<HealthFormsApiResponse<AddSessionMemberBulkStartResponse>> AddSessionMembers(string tenantToken, string tenantId, string sessionId, List<AddSessionMemberRequest> data, CancellationToken cancellationToken = default);
    Task<HealthFormsApiResponse<AddSessionMemberBulkResponse>> GetAddSessionMembersStatus(string tenantToken, string tenantId, string sessionId, string bulkId, CancellationToken cancellationToken = default);
    Task<HealthFormsApiResponse<SessionMemberResponse>> UpdateSessionMember(string tenantToken, string tenantId, string sessionId, UpdateSessionMemberRequest data, CancellationToken cancellationToken = default);
    Task<HealthFormsApiResponse> DeleteSessionMember(string tenantToken, string tenantId, string sessionId, string sessionMemberId, CancellationToken cancellationToken = default);
    Task<HealthFormsApiResponse> DeleteSessionMemberByExternalId(string tenantToken, string tenantId, string sessionId, string externalMemberId, CancellationToken cancellationToken = default);
    Task<HealthFormsApiResponse> DeleteSessionMemberByExternalAttendeeId(string tenantToken, string tenantId, string sessionId, string externalAttendeeId, CancellationToken cancellationToken = default);

    Task<HealthFormsApiResponse<List<WebhookSubscriptionResponse>>> GetWebhookSubscriptions(string tenantToken, string tenantId, CancellationToken cancellationToken = default);
    Task<HealthFormsApiResponse<WebhookSubscriptionResponse>> AddWebhookSubscription(string tenantToken, string tenantId, WebhookSubscriptionRequest data, CancellationToken cancellationToken = default);
    Task<HealthFormsApiResponse> DeleteWebhookSubscription(string tenantToken, string tenantId, string webhookId, CancellationToken cancellationToken = default);
}