﻿using System.Net;
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
            Log?.LogError(e, "Error getting the admin token. AuthHost: {HostAddressAuth} Message: {Message}", _options.HostAddressAuth, e.Message);
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

        Log?.LogCritical("Unable to claim HealthForms.io code. Reason: {Reason}  Message: {Message}", authTokenResponse.ErrorType, authTokenResponse.ErrorDescription);
        throw new HealthFormsAuthException("Unable to claim HealthForms.io code.", authTokenResponse);

    }

    #endregion

    #region Get Sessions
    
    public async Task<HealthFormsApiResponse<PagedResponse<List<SessionResponse>>>> GetSessions(string tenantToken, string tenantId, DateTime startDate, int page = 1, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(tenantToken)) throw new ArgumentNullException(nameof(tenantToken));
        if (string.IsNullOrWhiteSpace(tenantId)) throw new ArgumentNullException(nameof(tenantId));
        if (page < 1) throw new ArgumentOutOfRangeException(nameof(page));

        var response = await GetAsync<PagedResponse<List<SessionResponse>>>($"v1/{tenantId}/sessions?startDate={startDate.Date:yyyy-MM-dd}&page={page}", tenantToken, cancellationToken);
        response.Data ??= new PagedResponse<List<SessionResponse>>();
        return response;
    }

    public async Task<HealthFormsApiResponse<PagedResponse<List<SessionResponse>>>> GetSessions(string tenantToken, string nextUri, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(tenantToken)) throw new ArgumentNullException(nameof(tenantToken));
        if (string.IsNullOrWhiteSpace(nextUri)) throw new ArgumentNullException(nameof(nextUri));

        var response = await GetAsync<PagedResponse<List<SessionResponse>>>(nextUri, tenantToken, cancellationToken);
        response.Data ??= new PagedResponse<List<SessionResponse>>();
        return response;
    }
    
    public async Task<HealthFormsApiResponse<SessionResponse?>> GetSession(string tenantToken, string tenantId, string sessionId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(tenantToken)) throw new ArgumentNullException(nameof(tenantToken));
        if (string.IsNullOrWhiteSpace(tenantId)) throw new ArgumentNullException(nameof(tenantId));
        if (string.IsNullOrWhiteSpace(sessionId)) throw new ArgumentNullException(nameof(sessionId));

        var response = await GetAsync<SessionResponse?>($"v1/{tenantId}/sessions/{sessionId}", tenantToken, cancellationToken);
        return response;
    }

    public async Task<HealthFormsApiResponse<IEnumerable<SessionSelectResponse>>> GetSessionSelectList(string tenantToken, string tenantId, DateTime startDate, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(tenantToken)) throw new ArgumentNullException(nameof(tenantToken));
        if (string.IsNullOrWhiteSpace(tenantId)) throw new ArgumentNullException(nameof(tenantId));

        var response = await GetAsync<IEnumerable<SessionSelectResponse>>($"v1/{tenantId}/sessions/select?startDate={startDate}", tenantToken, cancellationToken);
        response.Data ??= new List<SessionSelectResponse>();
        return response;
    }

    #endregion

    #region Get SessionMembers

    public async Task<HealthFormsApiResponse<PagedResponse<List<SessionMemberResponse>>>> GetSessionMembers(string tenantToken, string tenantId, string sessionId, int page = 1, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(tenantToken)) throw new ArgumentNullException(nameof(tenantToken));
        if (string.IsNullOrWhiteSpace(tenantId)) throw new ArgumentNullException(nameof(tenantId));
        if (string.IsNullOrWhiteSpace(sessionId)) throw new ArgumentNullException(nameof(sessionId));
        if (page < 1) throw new ArgumentOutOfRangeException(nameof(page));

        var response = await GetAsync<PagedResponse<List<SessionMemberResponse>>>($"v1/{tenantId}/sessions/{sessionId}/members?page={page}", tenantToken, cancellationToken);
        response.Data ??= new PagedResponse<List<SessionMemberResponse>>();
        return response;
    }

    public async Task<HealthFormsApiResponse<PagedResponse<List<SessionMemberResponse>>>> GetSessionMembers(string tenantToken, string nextUri, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(tenantToken)) throw new ArgumentNullException(nameof(tenantToken));
        if (string.IsNullOrWhiteSpace(nextUri)) throw new ArgumentNullException(nameof(nextUri));

        var response = await GetAsync<PagedResponse<List<SessionMemberResponse>>>(nextUri, tenantToken, cancellationToken);
        response.Data ??= new PagedResponse<List<SessionMemberResponse>>();
        return response;
    }

    public async Task<HealthFormsApiResponse<SessionMemberResponse?>> GetSessionMember(string tenantToken, string tenantId, string sessionId, string sessionMemberId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(tenantToken)) throw new ArgumentNullException(nameof(tenantToken));
        if (string.IsNullOrWhiteSpace(tenantId)) throw new ArgumentNullException(nameof(tenantId));
        if (string.IsNullOrWhiteSpace(sessionId)) throw new ArgumentNullException(nameof(sessionId));
        if (string.IsNullOrWhiteSpace(sessionMemberId)) throw new ArgumentNullException(nameof(sessionMemberId));

        var response = await GetAsync<SessionMemberResponse?>($"v1/{tenantId}/sessions/{sessionId}/members/{sessionMemberId}", tenantToken, cancellationToken);
        return response;
    }

    public async Task<HealthFormsApiResponse<SessionMemberResponse?>> GetSessionMemberByExternalId(string tenantToken, string tenantId, string sessionId, string externalMemberId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(tenantToken)) throw new ArgumentNullException(nameof(tenantToken));
        if (string.IsNullOrWhiteSpace(tenantId)) throw new ArgumentNullException(nameof(tenantId));
        if (string.IsNullOrWhiteSpace(sessionId)) throw new ArgumentNullException(nameof(sessionId));
        if (string.IsNullOrWhiteSpace(externalMemberId)) throw new ArgumentNullException(nameof(externalMemberId));

        var response = await GetAsync<SessionMemberResponse?>($"v1/{tenantId}/sessions/{sessionId}/members/external/{externalMemberId}", tenantToken, cancellationToken);
        return response;
    }

    public async Task<HealthFormsApiResponse<SessionMemberResponse?>> GetSessionMemberByExternalAttendeeId(string tenantToken, string tenantId, string sessionId, string externalAttendeeId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(tenantToken)) throw new ArgumentNullException(nameof(tenantToken));
        if (string.IsNullOrWhiteSpace(tenantId)) throw new ArgumentNullException(nameof(tenantId));
        if (string.IsNullOrWhiteSpace(sessionId)) throw new ArgumentNullException(nameof(sessionId));
        if (string.IsNullOrWhiteSpace(externalAttendeeId)) throw new ArgumentNullException(nameof(externalAttendeeId));

        var response = await GetAsync<SessionMemberResponse?>($"v1/{tenantId}/sessions/{sessionId}/members/external-attendee/{externalAttendeeId}", tenantToken, cancellationToken);
        return response;
    }

    #endregion

    #region Find SessionMembers
    
    public async Task<HealthFormsApiResponse<SessionMemberSearchResponse>> SearchSessionMember(string tenantToken, string tenantId, string sessionId, SessionMemberSearchRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(tenantToken)) throw new ArgumentNullException(nameof(tenantToken));
        if (string.IsNullOrWhiteSpace(tenantId)) throw new ArgumentNullException(nameof(tenantId));
        if (string.IsNullOrWhiteSpace(sessionId)) throw new ArgumentNullException(nameof(sessionId));

        var response = await PostJsonAsync<SessionMemberSearchRequest, SessionMemberSearchResponse>($"v1/{tenantId}/sessions/{sessionId}/members/search", tenantToken, request, cancellationToken);
        return response;
    }
    

    #endregion

    #region Add SessionMember

    public async Task<HealthFormsApiResponse<SessionMemberResponse>> AddSessionMember(string tenantToken, string tenantId, string sessionId, AddSessionMemberRequest data, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(tenantToken)) throw new ArgumentNullException(nameof(tenantToken));
        if (string.IsNullOrWhiteSpace(tenantId)) throw new ArgumentNullException(nameof(tenantId));
        if (string.IsNullOrWhiteSpace(sessionId)) throw new ArgumentNullException(nameof(sessionId));

        return await PostJsonAsync<AddSessionMemberRequest, SessionMemberResponse>($"v1/{tenantId}/sessions/{sessionId}/members", tenantToken, data, cancellationToken);
    }

    public async Task<HealthFormsApiResponse<AddSessionMemberBulkStartResponse>> AddSessionMembers(string tenantToken, string tenantId, string sessionId, List<AddSessionMemberRequest> data, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(tenantToken)) throw new ArgumentNullException(nameof(tenantToken));
        if (string.IsNullOrWhiteSpace(tenantId)) throw new ArgumentNullException(nameof(tenantId));
        if (string.IsNullOrWhiteSpace(sessionId)) throw new ArgumentNullException(nameof(sessionId));
        if (data.Count > 1000) throw new HealthFormsException("The maximum number of members that can be added at once is 1000.");

        return await PostJsonAsync<List<AddSessionMemberRequest>, AddSessionMemberBulkStartResponse>($"v1/{tenantId}/sessions/{sessionId}/members/bulk", tenantToken, data, cancellationToken);
    }

    public async Task<HealthFormsApiResponse<AddSessionMemberBulkResponse>> GetAddSessionMembersStatus(string tenantToken, string tenantId, string sessionId, string bulkId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(tenantToken)) throw new ArgumentNullException(nameof(tenantToken));
        if (string.IsNullOrWhiteSpace(tenantId)) throw new ArgumentNullException(nameof(tenantId));
        if (string.IsNullOrWhiteSpace(sessionId)) throw new ArgumentNullException(nameof(sessionId));
        if (string.IsNullOrWhiteSpace(bulkId)) throw new ArgumentNullException(nameof(bulkId));

        return await GetAsync<AddSessionMemberBulkResponse>($"v1/{tenantId}/sessions/{sessionId}/members/bulk/{bulkId}", tenantToken, cancellationToken);
    }

    #endregion

    #region Update SessionMember

    public async Task<HealthFormsApiResponse<SessionMemberResponse>> UpdateSessionMember(string tenantToken, string tenantId, string sessionId, UpdateSessionMemberRequest data, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(tenantToken)) throw new ArgumentNullException(nameof(tenantToken));
        if (string.IsNullOrWhiteSpace(tenantId)) throw new ArgumentNullException(nameof(tenantId));
        if (string.IsNullOrWhiteSpace(sessionId)) throw new ArgumentNullException(nameof(sessionId));

        return await PutJsonAsync<UpdateSessionMemberRequest, SessionMemberResponse>($"v1/{tenantId}/sessions/{sessionId}/members", tenantToken, data, cancellationToken);
    }

    #endregion

    #region Delete SessionMembers

    public async Task<HealthFormsApiResponse> DeleteSessionMember(string tenantToken, string tenantId, string sessionId, string sessionMemberId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(tenantToken)) throw new ArgumentNullException(nameof(tenantToken));
        if (string.IsNullOrWhiteSpace(tenantId)) throw new ArgumentNullException(nameof(tenantId));
        if (string.IsNullOrWhiteSpace(sessionId)) throw new ArgumentNullException(nameof(sessionId));
        if (string.IsNullOrWhiteSpace(sessionMemberId)) throw new ArgumentNullException(nameof(sessionMemberId));

        return await DeleteAsync($"v1/{tenantId}/sessions/{sessionId}/members/{sessionMemberId}", tenantToken, cancellationToken);
    }

    public async Task<HealthFormsApiResponse> DeleteSessionMemberByExternalId(string tenantToken, string tenantId, string sessionId, string externalMemberId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(tenantToken)) throw new ArgumentNullException(nameof(tenantToken));
        if (string.IsNullOrWhiteSpace(tenantId)) throw new ArgumentNullException(nameof(tenantId));
        if (string.IsNullOrWhiteSpace(sessionId)) throw new ArgumentNullException(nameof(sessionId));
        if (string.IsNullOrWhiteSpace(externalMemberId)) throw new ArgumentNullException(nameof(externalMemberId));

        return await DeleteAsync($"v1/{tenantId}/sessions/{sessionId}/members/external/{externalMemberId}", tenantToken, cancellationToken);
    }

    public async Task<HealthFormsApiResponse> DeleteSessionMemberByExternalAttendeeId(string tenantToken, string tenantId, string sessionId, string externalAttendeeId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(tenantToken)) throw new ArgumentNullException(nameof(tenantToken));
        if (string.IsNullOrWhiteSpace(tenantId)) throw new ArgumentNullException(nameof(tenantId));
        if (string.IsNullOrWhiteSpace(sessionId)) throw new ArgumentNullException(nameof(sessionId));
        if (string.IsNullOrWhiteSpace(externalAttendeeId)) throw new ArgumentNullException(nameof(externalAttendeeId));

        return await DeleteAsync($"v1/{tenantId}/sessions/{sessionId}/members/external-attendee/{externalAttendeeId}", tenantToken, cancellationToken);
    }

    #endregion

    #region Webhook Subscriptions

    public async Task<HealthFormsApiResponse<List<WebhookSubscriptionResponse>>> GetWebhookSubscriptions(string tenantToken, string tenantId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(tenantToken)) throw new ArgumentNullException(nameof(tenantToken));
        if (string.IsNullOrWhiteSpace(tenantId)) throw new ArgumentNullException(nameof(tenantId));

        return await GetAsync<List<WebhookSubscriptionResponse>>($"v1/{tenantId}/webhooks/", tenantToken, cancellationToken);
    }

    public async Task<HealthFormsApiResponse<WebhookSubscriptionResponse>> AddWebhookSubscription(string tenantToken, string tenantId, WebhookSubscriptionRequest data, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(tenantToken)) throw new ArgumentNullException(nameof(tenantToken));
        if (string.IsNullOrWhiteSpace(tenantId)) throw new ArgumentNullException(nameof(tenantId));

        return await PostJsonAsync<WebhookSubscriptionRequest, WebhookSubscriptionResponse>($"v1/{tenantId}/webhooks/", tenantToken, data, cancellationToken);
    }

    public async Task<HealthFormsApiResponse> DeleteWebhookSubscription(string tenantToken, string tenantId, string webhookId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(tenantToken)) throw new ArgumentNullException(nameof(tenantToken));
        if (string.IsNullOrWhiteSpace(tenantId)) throw new ArgumentNullException(nameof(tenantId));
        if (string.IsNullOrWhiteSpace(webhookId)) throw new ArgumentNullException(nameof(webhookId));

        return await DeleteAsync($"v1/{tenantId}/webhooks/{webhookId}", tenantToken, cancellationToken);
    }

    #endregion

    #region Helpers

    #region Get

    private async Task<HealthFormsApiResponse<TResponse>> GetAsync<TResponse>(string route, string tenantToken, CancellationToken cancellationToken) where TResponse : class?
    {
        var accessToken = await GetAccessToken(tenantToken);
        HttpClient.SetBearerToken(accessToken.AccessToken);

        var response = await HttpClient.GetAsync($"{_options.HostAddressApi}{route}", cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var responseError = await LogOnErrorResponse(response);
            if (response.StatusCode == HttpStatusCode.NotFound) return new HealthFormsApiResponse<TResponse> { StatusCode = (int)response.StatusCode, ErrorMessage = "Not Found", Error = responseError};
            
            var errorMessage = responseError?.Message ?? $"The Get request failed with response code {response.StatusCode} to: {response.RequestMessage.RequestUri.OriginalString}";
            return new HealthFormsApiResponse<TResponse> { StatusCode = (int)response.StatusCode, ErrorMessage = errorMessage, Error = responseError};
        }

        try
        {
            var data = await response.Content.ReadFromJsonAsync<TResponse>(cancellationToken: cancellationToken, options: _jsonOptions);
            if (data == null) return new HealthFormsApiResponse<TResponse> { StatusCode = -1, ErrorMessage = "Unable to deserialize data." };
            return new HealthFormsApiResponse<TResponse> { Data = data, StatusCode = (int)response.StatusCode };
        }
        catch (Exception e)
        {
            var message = $"Unable to deserialize the response from the get request to: {response.RequestMessage.RequestUri.OriginalString}.";
            var responseString = await response.Content.ReadAsStringAsync();
            Log?.LogError(e, "{Message}, Data: {Data}", message, responseString);
            return new HealthFormsApiResponse<TResponse> { StatusCode = -1, ErrorMessage = $"Unable to deserialize the response from the get request to: {response.RequestMessage.RequestUri.OriginalString}." };
        }

    }

    #endregion

    #region Post

    protected async Task<HealthFormsApiResponse<TResponse>> PostJsonAsync<TRequest, TResponse>(string route, string tenantToken, TRequest requestData, CancellationToken cancellationToken = default) where TRequest : class where TResponse : class
    {
        var accessToken = await GetAccessToken(tenantToken);
        HttpClient.SetBearerToken(accessToken.AccessToken);

        var response = await HttpClient.PostAsJsonAsync($"{_options.HostAddressApi}{route}", requestData, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var responseError = await LogOnErrorResponse(response);

            var errorMessage = responseError?.Message ?? $"The Post request failed with response code {response.StatusCode} to: {response.RequestMessage.RequestUri.OriginalString}.";
            return new HealthFormsApiResponse<TResponse> { StatusCode = (int)response.StatusCode, ErrorMessage = errorMessage, Error = responseError };
        }

        try
        {
            var data = await response.Content.ReadFromJsonAsync<TResponse>(cancellationToken: cancellationToken, options: _jsonOptions);
            return data == null 
                ? new HealthFormsApiResponse<TResponse> { StatusCode = -1, ErrorMessage = "Unable to deserialize data." } 
                : new HealthFormsApiResponse<TResponse> { Data = data, StatusCode = (int)response.StatusCode };
        }
        catch (Exception e)
        {
            var message = $"Unable to deserialize the response from the post request to: {response.RequestMessage.RequestUri.OriginalString}.";
            var responseString = await response.Content.ReadAsStringAsync();
            Log?.LogError(e, "{Message}, Data: {Data}", message, responseString);
            return new HealthFormsApiResponse<TResponse> { StatusCode = -1, ErrorMessage = $"Unable to deserialize the response from the post request to: {response.RequestMessage.RequestUri.OriginalString}." };
        }

    }

    #endregion

    #region Put

    protected async Task<HealthFormsApiResponse<TResponse>> PutJsonAsync<TRequest, TResponse>(string route, string tenantToken, TRequest requestData, CancellationToken cancellationToken = default) where TRequest : class where TResponse : class
    {
        var accessToken = await GetAccessToken(tenantToken);
        HttpClient.SetBearerToken(accessToken.AccessToken);

        var response = await HttpClient.PutAsJsonAsync($"{_options.HostAddressApi}{route}", requestData, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var responseError = await LogOnErrorResponse(response);
            var errorMessage = responseError?.Message ?? $"The Put request failed with response code {response.StatusCode} to: {response.RequestMessage.RequestUri.OriginalString}.";
            return new HealthFormsApiResponse<TResponse> { StatusCode = (int)response.StatusCode, ErrorMessage = errorMessage, Error = responseError };
        }

        try
        {
            var data = await response.Content.ReadFromJsonAsync<TResponse>(cancellationToken: cancellationToken, options: _jsonOptions);
            if (data == null) return new HealthFormsApiResponse<TResponse> { StatusCode = -1, ErrorMessage = "Unable to deserialize data." };
            return new HealthFormsApiResponse<TResponse> { Data = data, StatusCode = (int)response.StatusCode };
        }
        catch (Exception e)
        {
            var message = $"Unable to deserialize the response from the put request to: {response.RequestMessage.RequestUri.OriginalString}.";
            var responseString = await response.Content.ReadAsStringAsync();
            Log?.LogError(e, "{Message}, Data: {Data}", message, responseString);
            return new HealthFormsApiResponse<TResponse> { StatusCode = -1, ErrorMessage = $"Unable to deserialize the response from the put request to: {response.RequestMessage.RequestUri.OriginalString}." };
        }

    }

    #endregion

    #region Delete

    protected async Task<HealthFormsApiResponse> DeleteAsync(string route, string tenantToken, CancellationToken cancellationToken = default)
    {
        var accessToken = await GetAccessToken(tenantToken);
            HttpClient.SetBearerToken(accessToken.AccessToken);

        var response = await HttpClient.DeleteAsync($"{_options.HostAddressApi}{route}", cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var responseError = await LogOnErrorResponse(response);
            var errorMessage = responseError?.Message ?? $"The Put request failed with response code {response.StatusCode} to: {response.RequestMessage.RequestUri.OriginalString}.";
            return new HealthFormsApiResponse { StatusCode = (int)response.StatusCode, ErrorMessage = errorMessage, Error = responseError };

        }

        return new HealthFormsApiResponse { StatusCode = (int)response.StatusCode };
    }

    #endregion

    #region Logging

    private async Task<HealthFormsErrorResponse?> LogOnErrorResponse(HttpResponseMessage response)
    {

        var responseString = await response.Content.ReadAsStringAsync();
        HealthFormsErrorResponse? errorResponse = null;
        if (response.StatusCode != HttpStatusCode.NotFound)
        {
            try
            {
                errorResponse = await response.Content.ReadFromJsonAsync<HealthFormsErrorResponse>();
            }
            catch (JsonException)
            {
                //ignore
            }
        }

        if (errorResponse != null)
        {
            Log?.LogError("HealthForms Request Error at: {Address} Response: {ResponseString}", response.RequestMessage?.RequestUri?.ToString() ?? "Unknown HealthForms Request Address", responseString);
            return errorResponse;
        }
        if (!response.IsSuccessStatusCode || responseString.Contains("\"status\":\"failure\""))
            Log?.LogError("HealthForms Request Error at: {Address} Response: {ResponseString}", response.RequestMessage?.RequestUri?.ToString() ?? "Unknown HealthForms Request Address", responseString);

        return null;
    }

    #endregion

    #endregion
}