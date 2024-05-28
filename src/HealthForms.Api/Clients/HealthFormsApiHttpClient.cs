using System.Net;
using System.Net.Http.Json;
using HealthForms.Api.Options;
using Microsoft.Extensions.Options;
using HealthForms.Api.Errors;
using IdentityModel.Client;
using Microsoft.Extensions.Logging;
using IdentityModel;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using HealthForms.Api.Core.Models;
using HealthForms.Api.Core.Models.Auth;
using HealthForms.Api.Core.Models.Errors;
using HealthForms.Api.Core.Models.SessionMember;
using HealthForms.Api.Core.Models.Sessions;
using HealthForms.Api.Core.Models.Webhooks;
using HealthForms.Api.Shared;

namespace HealthForms.Api.Clients;

public class HealthFormsApiHttpClient : IHealthFormsApiHttpClient
{
    private ILogger<HealthFormsApiHttpClient>? Log { get; }
    protected readonly HttpClient HttpClient;
    private readonly HealthFormsApiOptions _options;


    private readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, ReferenceHandler = ReferenceHandler.IgnoreCycles };


    public HealthFormsApiHttpClient(HttpClient httpClient, IOptions<HealthFormsApiOptions> options, ILogger<HealthFormsApiHttpClient>? log = null)
    {
        Log = log;
        HttpClient = httpClient;
        _options = options.Value;

        _jsonOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
    }

    #region Access Token

    public async Task<AuthResponse> GetAccessToken(string tenantToken)
    {
        TokenCache.TryGetValue(tenantToken, out var accessToken);
        if (accessToken!=null && accessToken.ExpiresOn >= DateTime.UtcNow) return accessToken;

        try
        {
            var response = await HttpClient.RequestRefreshTokenAsync(new RefreshTokenRequest
            {
                Address = $"{_options.HostAddressAuth}connect/token",
                ClientId = _options.ClientId,
                ClientSecret = _options.ClientSecret,
                RefreshToken = tenantToken, ClientCredentialStyle = ClientCredentialStyle.PostBody
            });

            if (response.IsError) throw new HealthFormsAuthException("Unable to get access token.", response);
            accessToken = new AuthResponse(response);
            TokenCache.Set(tenantToken, accessToken);

            return accessToken;
        }
        catch (HealthFormsException e)
        {
            Log?.LogError(e, e.Message);
            throw;
        }
        catch (Exception e)
        {
            Log?.LogError(e, $"Error getting the admin token. AuthHost: {_options.HostAddressAuth} Message: {e.Message}");
            throw new HealthFormsException($"Error getting the admin token. AuthHost: {_options.HostAddressAuth} Message: {e.Message}");
        }
    }

    #endregion

    #region Claim Code

    public AuthRedirect GetRedirectUrl(string tenantId, string? redirectUrl = null)
    {
        if (tenantId == null) throw new ArgumentNullException(nameof(tenantId));

        var codeVerifier = GenerateCodeVerifier();
        var codeChallenge = GenerateCodeChallenge(codeVerifier);

        var url = new RequestUrl($"{_options.HostAddressAuth}connect/authorize")
            .CreateAuthorizeUrl(
                clientId: _options.ClientId,
                responseType: OidcConstants.ResponseTypes.Code,
                scope: _options.Scopes.Replace("{{tenantId}}", tenantId),
                redirectUri: string.IsNullOrWhiteSpace(redirectUrl) ? _options.RedirectUrl : redirectUrl,
                codeChallenge: codeChallenge,
                codeChallengeMethod: OidcConstants.CodeChallengeMethods.Sha256,
                nonce: CryptoRandom.CreateUniqueId(),
                state: CryptoRandom.CreateUniqueId());

        return new AuthRedirect { CodeVerifier = codeVerifier, Uri = url };
    }

    private static string GenerateCodeVerifier()
    {
        const int codeVerifierLength = 64; // You can choose a length between 43 and 128
        using var rng = new RNGCryptoServiceProvider();
        var bytes = new byte[codeVerifierLength];
        rng.GetBytes(bytes);
        return Convert.ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }

    private static string GenerateCodeChallenge(string codeVerifier)
    {
        using var sha256 = SHA256.Create();
        var challengeBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(codeVerifier));
        return Convert.ToBase64String(challengeBytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }

    public string GetTenantIdFromScope(string scope)
    {
        var scopes = scope.Split(' ').ToList();
        var tenantId = scopes.Find(s => s.StartsWith("customer:"));

        return tenantId == null 
            ? throw new HealthFormsException("The scope does not contain a tenantId.") 
            : tenantId.Split(':')[1];
    }

    public async Task<string> GetTenantToken(string code, string codeVerifier, string? redirectUrl = null, CancellationToken cancellationToken = default)
    {
        var authToken = await ClaimCode(code, codeVerifier, redirectUrl, cancellationToken);
        if (string.IsNullOrWhiteSpace(authToken.RefreshToken))
        {
            Log?.LogCritical("Unable to get tenant HealthForms.io access token.");
            throw new HealthFormsAuthException("Unable to get tenant HealthForms.io access token.", authToken);
        }

        return authToken.RefreshToken ?? string.Empty;
    }

    public async Task<TokenResponse> ClaimCode(string code, string codeVerifier, string? redirectUrl = null, CancellationToken cancellationToken = default)
    {
        var authTokenResponse = await HttpClient.RequestAuthorizationCodeTokenAsync(new AuthorizationCodeTokenRequest
        {
            Address = $"{_options.HostAddressAuth}connect/token",
            ClientId = _options.ClientId,
            ClientSecret = _options.ClientSecret,
            Code = code,
            RedirectUri = redirectUrl ?? _options.RedirectUrl,

            // optional PKCE parameter
            CodeVerifier = codeVerifier, ClientCredentialStyle = ClientCredentialStyle.PostBody
        }, cancellationToken: cancellationToken);

        if (authTokenResponse is { IsError: false, RefreshToken: not null })
        {
            var authToken = new AuthResponse(authTokenResponse);
            TokenCache.Set(authTokenResponse.RefreshToken, authToken);
            return authTokenResponse;
        }

        Log?.LogCritical("Unable to claim HealthForms.io code. Reason: {reason}  Message: {message}", authTokenResponse.ErrorType, authTokenResponse.ErrorDescription);
        throw new HealthFormsAuthException("Unable to claim HealthForms.io code.", authTokenResponse);

    }

    #endregion

    #region Get Sessions
    
    public async Task<PagedResponse<List<SessionResponse>>> GetSessions(string tenantToken, string tenantId, DateTime startDate, int page = 1, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(tenantToken)) throw new ArgumentNullException(nameof(tenantToken));
        if (string.IsNullOrWhiteSpace(tenantId)) throw new ArgumentNullException(nameof(tenantId));
        if (page < 1) throw new ArgumentOutOfRangeException(nameof(page));

        return await GetAsync<PagedResponse<List<SessionResponse>>>($"v1/{tenantId}/sessions?startDate={startDate.Date:yyyy-MM-dd}&page={page}", tenantToken, cancellationToken);
    }

    public async Task<PagedResponse<List<SessionResponse>>> GetSessions(string tenantToken, string nextUri, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(tenantToken)) throw new ArgumentNullException(nameof(tenantToken));
        if (string.IsNullOrWhiteSpace(nextUri)) throw new ArgumentNullException(nameof(nextUri));

        return await GetAsync<PagedResponse<List<SessionResponse>>>(nextUri, tenantToken, cancellationToken);
    }
    
    public async Task<SessionResponse?> GetSession(string tenantToken, string tenantId, string sessionId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(tenantToken)) throw new ArgumentNullException(nameof(tenantToken));
        if (string.IsNullOrWhiteSpace(tenantId)) throw new ArgumentNullException(nameof(tenantId));
        if (string.IsNullOrWhiteSpace(sessionId)) throw new ArgumentNullException(nameof(sessionId));

        return await GetAsync<SessionResponse?>($"v1/{tenantId}/sessions/{sessionId}", tenantToken, cancellationToken);
    }

    public async Task<IEnumerable<SessionSelectResponse>> GetSessionSelectList(string tenantToken, string tenantId, DateTime startDate, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(tenantToken)) throw new ArgumentNullException(nameof(tenantToken));
        if (string.IsNullOrWhiteSpace(tenantId)) throw new ArgumentNullException(nameof(tenantId));

        var sessions = await GetAsync<IEnumerable<SessionSelectResponse>?>($"v1/{tenantId}/sessions/select?startDate={startDate}", tenantToken, cancellationToken);
        return sessions ?? new List<SessionSelectResponse>();
    }

    #endregion

    #region Get SessionMembers

    public async Task<PagedResponse<List<SessionMemberResponse>>> GetSessionMembers(string tenantToken, string tenantId, string sessionId, int page = 1, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(tenantToken)) throw new ArgumentNullException(nameof(tenantToken));
        if (string.IsNullOrWhiteSpace(tenantId)) throw new ArgumentNullException(nameof(tenantId));
        if (string.IsNullOrWhiteSpace(sessionId)) throw new ArgumentNullException(nameof(sessionId));
        if (page < 1) throw new ArgumentOutOfRangeException(nameof(page));

        return await GetAsync<PagedResponse<List<SessionMemberResponse>>>($"v1/{tenantId}/sessions/{sessionId}/members?page={page}", tenantToken, cancellationToken);
    }

    public async Task<PagedResponse<List<SessionMemberResponse>>> GetSessionMembers(string tenantToken, string nextUri, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(tenantToken)) throw new ArgumentNullException(nameof(tenantToken));
        if (string.IsNullOrWhiteSpace(nextUri)) throw new ArgumentNullException(nameof(nextUri));

        return await GetAsync<PagedResponse<List<SessionMemberResponse>>>(nextUri, tenantToken, cancellationToken);
    }

    public async Task<SessionMemberResponse?> GetSessionMember(string tenantToken, string tenantId, string sessionId, string sessionMemberId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(tenantToken)) throw new ArgumentNullException(nameof(tenantToken));
        if (string.IsNullOrWhiteSpace(tenantId)) throw new ArgumentNullException(nameof(tenantId));
        if (string.IsNullOrWhiteSpace(sessionId)) throw new ArgumentNullException(nameof(sessionId));
        if (string.IsNullOrWhiteSpace(sessionMemberId)) throw new ArgumentNullException(nameof(sessionMemberId));

        return await GetAsync<SessionMemberResponse?>($"v1/{tenantId}/sessions/{sessionId}/members/{sessionMemberId}", tenantToken, cancellationToken);
    }

    public async Task<SessionMemberResponse?> GetSessionMemberByExternalId(string tenantToken, string tenantId, string sessionId, string externalMemberId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(tenantToken)) throw new ArgumentNullException(nameof(tenantToken));
        if (string.IsNullOrWhiteSpace(tenantId)) throw new ArgumentNullException(nameof(tenantId));
        if (string.IsNullOrWhiteSpace(sessionId)) throw new ArgumentNullException(nameof(sessionId));
        if (string.IsNullOrWhiteSpace(externalMemberId)) throw new ArgumentNullException(nameof(externalMemberId));

        return await GetAsync<SessionMemberResponse?>($"v1/{tenantId}/sessions/{sessionId}/members/external/{externalMemberId}", tenantToken, cancellationToken);
    }

    public async Task<SessionMemberResponse?> GetSessionMemberByExternalAttendeeId(string tenantToken, string tenantId, string sessionId, string externalAttendeeId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(tenantToken)) throw new ArgumentNullException(nameof(tenantToken));
        if (string.IsNullOrWhiteSpace(tenantId)) throw new ArgumentNullException(nameof(tenantId));
        if (string.IsNullOrWhiteSpace(sessionId)) throw new ArgumentNullException(nameof(sessionId));
        if (string.IsNullOrWhiteSpace(externalAttendeeId)) throw new ArgumentNullException(nameof(externalAttendeeId));

        return await GetAsync<SessionMemberResponse?>($"v1/{tenantId}/sessions/{sessionId}/members/external-attendee/{externalAttendeeId}", tenantToken, cancellationToken);
    }

    #endregion

    #region Add SessionMember

    public async Task<SessionMemberResponse> AddSessionMember(string tenantToken, string tenantId, string sessionId, AddSessionMemberRequest data, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(tenantToken)) throw new ArgumentNullException(nameof(tenantToken));
        if (string.IsNullOrWhiteSpace(tenantId)) throw new ArgumentNullException(nameof(tenantId));
        if (string.IsNullOrWhiteSpace(sessionId)) throw new ArgumentNullException(nameof(sessionId));

        return await PostJsonAsync<AddSessionMemberRequest, SessionMemberResponse>($"v1/{tenantId}/sessions/{sessionId}/members", tenantToken, data, cancellationToken);
    }

    public async Task<AddSessionMemberBulkStartResponse> AddSessionMembers(string tenantToken, string tenantId, string sessionId, List<AddSessionMemberRequest> data, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(tenantToken)) throw new ArgumentNullException(nameof(tenantToken));
        if (string.IsNullOrWhiteSpace(tenantId)) throw new ArgumentNullException(nameof(tenantId));
        if (string.IsNullOrWhiteSpace(sessionId)) throw new ArgumentNullException(nameof(sessionId));
        if (data.Count > 1000) throw new HealthFormsException("The maximum number of members that can be added at once is 1000.");

        return await PostJsonAsync<List<AddSessionMemberRequest>, AddSessionMemberBulkStartResponse>($"v1/{tenantId}/sessions/{sessionId}/members/bulk", tenantToken, data, cancellationToken);
    }

    public async Task<AddSessionMemberBulkResponse> GetAddSessionMembersStatus(string tenantToken, string tenantId, string sessionId, string bulkId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(tenantToken)) throw new ArgumentNullException(nameof(tenantToken));
        if (string.IsNullOrWhiteSpace(tenantId)) throw new ArgumentNullException(nameof(tenantId));
        if (string.IsNullOrWhiteSpace(sessionId)) throw new ArgumentNullException(nameof(sessionId));
        if (string.IsNullOrWhiteSpace(bulkId)) throw new ArgumentNullException(nameof(bulkId));

        return await GetAsync<AddSessionMemberBulkResponse>($"v1/{tenantId}/sessions/{sessionId}/members/bulk/{bulkId}", tenantToken, cancellationToken);
    }

    #endregion

    #region Update SessionMember

    public async Task<SessionMemberResponse> UpdateSessionMember(string tenantToken, string tenantId, string sessionId, UpdateSessionMemberRequest data, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(tenantToken)) throw new ArgumentNullException(nameof(tenantToken));
        if (string.IsNullOrWhiteSpace(tenantId)) throw new ArgumentNullException(nameof(tenantId));
        if (string.IsNullOrWhiteSpace(sessionId)) throw new ArgumentNullException(nameof(sessionId));

        return await PutJsonAsync<UpdateSessionMemberRequest, SessionMemberResponse>($"v1/{tenantId}/sessions/{sessionId}/members", tenantToken, data, cancellationToken);
    }

    #endregion

    #region Delete SessionMembers

    public async Task<bool> DeleteSessionMember(string tenantToken, string tenantId, string sessionId, string sessionMemberId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(tenantToken)) throw new ArgumentNullException(nameof(tenantToken));
        if (string.IsNullOrWhiteSpace(tenantId)) throw new ArgumentNullException(nameof(tenantId));
        if (string.IsNullOrWhiteSpace(sessionId)) throw new ArgumentNullException(nameof(sessionId));
        if (string.IsNullOrWhiteSpace(sessionMemberId)) throw new ArgumentNullException(nameof(sessionMemberId));

        return await DeleteAsync($"v1/{tenantId}/sessions/{sessionId}/members/{sessionMemberId}", tenantToken, cancellationToken);
    }

    public async Task<bool> DeleteSessionMemberByExternalId(string tenantToken, string tenantId, string sessionId, string externalMemberId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(tenantToken)) throw new ArgumentNullException(nameof(tenantToken));
        if (string.IsNullOrWhiteSpace(tenantId)) throw new ArgumentNullException(nameof(tenantId));
        if (string.IsNullOrWhiteSpace(sessionId)) throw new ArgumentNullException(nameof(sessionId));
        if (string.IsNullOrWhiteSpace(externalMemberId)) throw new ArgumentNullException(nameof(externalMemberId));

        return await DeleteAsync($"v1/{tenantId}/sessions/{sessionId}/members/external/{externalMemberId}", tenantToken, cancellationToken);
    }

    public async Task<bool> DeleteSessionMemberByExternalAttendeeId(string tenantToken, string tenantId, string sessionId, string externalAttendeeId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(tenantToken)) throw new ArgumentNullException(nameof(tenantToken));
        if (string.IsNullOrWhiteSpace(tenantId)) throw new ArgumentNullException(nameof(tenantId));
        if (string.IsNullOrWhiteSpace(sessionId)) throw new ArgumentNullException(nameof(sessionId));
        if (string.IsNullOrWhiteSpace(externalAttendeeId)) throw new ArgumentNullException(nameof(externalAttendeeId));

        return await DeleteAsync($"v1/{tenantId}/sessions/{sessionId}/members/external-attendee/{externalAttendeeId}", tenantToken, cancellationToken);
    }

    #endregion

    #region Webhook Subscriptions

    public async Task<List<WebhookSubscriptionResponse>> GetWebhookSubscriptions(string tenantToken, string tenantId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(tenantToken)) throw new ArgumentNullException(nameof(tenantToken));
        if (string.IsNullOrWhiteSpace(tenantId)) throw new ArgumentNullException(nameof(tenantId));

        return await GetAsync<List<WebhookSubscriptionResponse>>($"v1/{tenantId}/webhooks/", tenantToken, cancellationToken);
    }

    public async Task<WebhookSubscriptionResponse> AddWebhookSubscription(string tenantToken, string tenantId, WebhookSubscriptionRequest data, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(tenantToken)) throw new ArgumentNullException(nameof(tenantToken));
        if (string.IsNullOrWhiteSpace(tenantId)) throw new ArgumentNullException(nameof(tenantId));

        return await PostJsonAsync<WebhookSubscriptionRequest, WebhookSubscriptionResponse>($"v1/{tenantId}/webhooks/", tenantToken, data, cancellationToken);
    }

    public async Task<bool> DeleteWebhookSubscription(string tenantToken, string tenantId, string webhookId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(tenantToken)) throw new ArgumentNullException(nameof(tenantToken));
        if (string.IsNullOrWhiteSpace(tenantId)) throw new ArgumentNullException(nameof(tenantId));
        if (string.IsNullOrWhiteSpace(webhookId)) throw new ArgumentNullException(nameof(webhookId));

        return await DeleteAsync($"v1/{tenantId}/webhooks/{webhookId}", tenantToken, cancellationToken);
    }

    #endregion

    #region Helpers

    #region Get

    private async Task<TResponse> GetAsync<TResponse>(string route, string tenantToken, CancellationToken cancellationToken) where TResponse : class?
    {
        var accessToken = await GetAccessToken(tenantToken);
        HttpClient.SetBearerToken(accessToken.AccessToken);

        var response = await HttpClient.GetAsync($"{_options.HostAddressApi}{route}", cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var responseError = await LogOnErrorResponse(response);
            if (response.StatusCode == HttpStatusCode.NotFound) return null!;

            if (responseError != null) throw new HealthFormsException(responseError);
            throw new HealthFormsException($"The Get request failed with response code {response.StatusCode} to: {response.RequestMessage.RequestUri.OriginalString}");
        }

        try
        {
            return await response.Content.ReadFromJsonAsync<TResponse>(cancellationToken: cancellationToken, options: _jsonOptions)
                   ?? throw new HealthFormsException("Unable deserialize.");
        }
        catch (Exception)
        {
            var message = $"Unable to deserialize the response from the get request to: {response.RequestMessage.RequestUri.OriginalString}.";
            var responseString = await response.Content.ReadAsStringAsync();
            Log?.LogError("{message}, Data: {data}", message, responseString);
            throw new HealthFormsException($"Unable to deserialize the response from the get request to: {response.RequestMessage.RequestUri.OriginalString}.");
        }

    }

    #endregion

    #region Post

    protected async Task<TResponse> PostJsonAsync<TRequest, TResponse>(string route, string tenantToken, TRequest data, CancellationToken cancellationToken = default) where TRequest : class where TResponse : class
    {
        var accessToken = await GetAccessToken(tenantToken);
        HttpClient.SetBearerToken(accessToken.AccessToken);

        var response = await HttpClient.PostAsJsonAsync($"{_options.HostAddressApi}{route}", data, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var responseError = await LogOnErrorResponse(response);
            if (responseError != null) throw new HealthFormsException(responseError);
            throw new HealthFormsException($"The Post request failed with response code {response.StatusCode} to: {response.RequestMessage.RequestUri.OriginalString}.");
        }

        try
        {
            return await response.Content.ReadFromJsonAsync<TResponse>(cancellationToken: cancellationToken, options: _jsonOptions)
                   ?? throw new HealthFormsException("Unable deserialize.");
        }
        catch (Exception)
        {
            var message = $"Unable to deserialize the response from the post request to: {response.RequestMessage.RequestUri.OriginalString}.";
            var responseString = await response.Content.ReadAsStringAsync();
            Log?.LogError("{message}, Data: {data}", message, responseString);
            throw new HealthFormsException($"Unable to deserialize the response from the post request to: {response.RequestMessage.RequestUri.OriginalString}.");
        }

    }

    #endregion

    #region Put

    protected async Task<TResponse> PutJsonAsync<TRequest, TResponse>(string route, string tenantToken, TRequest data, CancellationToken cancellationToken = default) where TRequest : class where TResponse : class
    {
        var accessToken = await GetAccessToken(tenantToken);
        HttpClient.SetBearerToken(accessToken.AccessToken);

        var response = await HttpClient.PutAsJsonAsync($"{_options.HostAddressApi}{route}", data, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var responseError = await LogOnErrorResponse(response);
            if (responseError != null) throw new HealthFormsException(responseError);
            throw new HealthFormsException($"The Put request failed with response code {response.StatusCode} to: {response.RequestMessage.RequestUri.OriginalString}.");
        }

        try
        {
            return await response.Content.ReadFromJsonAsync<TResponse>(cancellationToken: cancellationToken, options: _jsonOptions)
                   ?? throw new HealthFormsException("Unable deserialize.");
        }
        catch (Exception)
        {
            var message = $"Unable to deserialize the response from the put request to: {response.RequestMessage.RequestUri.OriginalString}.";
            var responseString = await response.Content.ReadAsStringAsync();
            Log?.LogError("{message}, Data: {data}", message, responseString);
            throw new HealthFormsException($"Unable to deserialize the response from the put request to: {response.RequestMessage.RequestUri.OriginalString}.");
        }

    }

    #endregion

    #region Delete

    protected async Task<bool> DeleteAsync(string route, string tenantToken, CancellationToken cancellationToken = default)
    {
        var accessToken = await GetAccessToken(tenantToken);
            HttpClient.SetBearerToken(accessToken.AccessToken);

        var response = await HttpClient.DeleteAsync($"{_options.HostAddressApi}{route}", cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var responseError = await LogOnErrorResponse(response);
            if (responseError != null) throw new HealthFormsException(responseError);
            throw new HealthFormsException($"The Delete request failed with response code {response.StatusCode} to: {response.RequestMessage.RequestUri.OriginalString}.");

        }

        return response.IsSuccessStatusCode;
    }

    #endregion

    #region Logging

    private async Task<HealthFormsErrorResponse?> LogOnErrorResponse(HttpResponseMessage response)
    {

        var responseString = await response.Content.ReadAsStringAsync();
        HealthFormsErrorResponse? errorResponse = null;
        if (response.StatusCode != HttpStatusCode.NotFound)
        {
            errorResponse = await response.Content.ReadFromJsonAsync<HealthFormsErrorResponse>();
        }

        if (errorResponse != null)
        {
            Log?.LogError($"HealthForms Request Error at: {response.RequestMessage?.RequestUri?.ToString() ?? "Unknown HealthForms Request Address"} Response: {responseString}");
            return errorResponse;
        }
        if (!response.IsSuccessStatusCode || responseString.Contains("\"status\":\"failure\""))
        {
            Log?.LogError($"HealthForms Request Error at: {response.RequestMessage?.RequestUri?.ToString() ?? "Unknown HealthForms Request Address"} Response: {responseString}");
        }

        return null;
    }

    #endregion

    #endregion
}