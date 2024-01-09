using HealthForms.Api.Options;
using Microsoft.Extensions.Options;
using HealthForms.Api.Errors;
using IdentityModel.Client;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using IdentityModel;
using System.Security.Cryptography;
using System.Text;
using HealthForms.Api.Core.Models.Auth;

namespace HealthForms.Api.Clients;

public class HealthFormsApiHttpClient
{
    private ILogger<HealthFormsApiHttpClient> Log { get; }
    private readonly HttpClient _httpClient;
    private readonly IMemoryCache _memoryCache;
    private readonly HealthFormsApiOptions _options;
    private AuthResponse? _accessToken;

    public HealthFormsApiHttpClient(HttpClient httpClient, IOptions<HealthFormsApiOptions> options, IMemoryCache memoryCache, ILogger<HealthFormsApiHttpClient> log)
    {
        Log = log;
        _httpClient = httpClient;
        _memoryCache = memoryCache;
        _options = options.Value;
    }

    #region Access Token

    public async Task<AuthResponse> GetAccessToken(string tenantToken)
    {
        _memoryCache.TryGetValue(tenantToken, out AuthResponse? _accessToken);
        if (_accessToken!=null && _accessToken.ExpiresOn >= DateTime.UtcNow) return _accessToken;

        try
        {
            var response = await _httpClient.RequestRefreshTokenAsync(new RefreshTokenRequest
            {
                Address = _options.HostAddressAuth,
                ClientId = _options.ClientId,
                ClientSecret = _options.ClientSecret,
                RefreshToken = tenantToken
            });

            if (response.IsError) throw new HealthFormsAuthException("Unable to claim HealthForms.io code.", response);
            _accessToken = new AuthResponse(response);
            _memoryCache.Set(tenantToken, _accessToken, TimeSpan.FromSeconds(response.ExpiresIn-10));

            return _accessToken;
        }
        catch (HealthFormsException e)
        {
            Log.LogError(e, e.Message);
            throw;
        }
        catch (Exception e)
        {
            Log.LogError(e, $"Error getting the admin token. AuthHost: {_options.HostAddressAuth} Message: {e.Message}");
            throw new HealthFormsException($"Error getting the admin token. AuthHost: {_options.HostAddressAuth} Message: {e.Message}");
        }
    }

    #endregion

    #region Claim Code

    public AuthRedirect GetRedirectUrl(string tenantId)
    {
        if (tenantId == null) throw new ArgumentNullException(nameof(tenantId));

        var codeVerifier = GenerateCodeVerifier();
        var codeChallenge = GenerateCodeChallenge(codeVerifier);

        var url = new RequestUrl($"{_options.HostAddressAuth}connect/authorize")
            .CreateAuthorizeUrl(
                clientId: _options.ClientId,
                responseType: OidcConstants.ResponseTypes.Code,
                scope: _options.Scopes.Replace("{{tenantId}}", tenantId),
                redirectUri: _options.RedirectUrl,
                codeChallenge: codeChallenge,
                codeChallengeMethod: OidcConstants.CodeChallengeMethods.Sha256,
                nonce: CryptoRandom.CreateUniqueId(32),
                state: CryptoRandom.CreateUniqueId(32));

        return new AuthRedirect { CodeVerifier = codeVerifier, Uri = url };
    }

    public static string GenerateCodeVerifier()
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

    public static string GenerateCodeChallenge(string codeVerifier)
    {
        using var sha256 = SHA256.Create();
        var challengeBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(codeVerifier));
        return Convert.ToBase64String(challengeBytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }

    public async Task<string> GetTenantToken(string code, string codeVerifier, CancellationToken cancellationToken = default)
    {
        var authToken = await ClaimCode(code, codeVerifier, cancellationToken);
        if (string.IsNullOrWhiteSpace(authToken.RefreshToken)) Log.LogCritical("Unable to get tenant HealthForms.io access token.");

        return authToken.RefreshToken ?? string.Empty;
    }

    public async Task<TokenResponse> ClaimCode(string code, string codeVerifier, CancellationToken cancellationToken = default)
    {
        var authTokenResponse = await _httpClient.RequestAuthorizationCodeTokenAsync(new AuthorizationCodeTokenRequest
        {
            Address = $"{_options.HostAddressAuth}connect/token",
            ClientId = _options.ClientId,
            ClientSecret = _options.ClientSecret,
            Code = code,
            RedirectUri = _options.RedirectUrl,

            // optional PKCE parameter
            CodeVerifier = codeVerifier, ClientCredentialStyle = ClientCredentialStyle.PostBody
        }, cancellationToken: cancellationToken);

        if (!authTokenResponse.IsError)
        {
            _memoryCache.Set(authTokenResponse.RefreshToken, _accessToken, TimeSpan.FromSeconds(authTokenResponse.ExpiresIn-10));
            return authTokenResponse;
        }

        Log.LogCritical("Unable to claim HealthForms.io code. Reason: {reason}  Message: {message}", authTokenResponse.ErrorType, authTokenResponse.ErrorDescription);
        throw new HealthFormsAuthException("Unable to claim HealthForms.io code.", authTokenResponse);

    }

    #endregion

    #region Logging

    private async Task LogOnErrorResponse(HttpResponseMessage response)
    {

        var responseString = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode || responseString.Contains("\"status\":\"failure\""))
        {
            Log.LogError($"HealthForms Request Error at: {response.RequestMessage?.RequestUri?.ToString() ?? "Unknown HealthForms Request Address"} Response: {responseString}");
        }
    }

    #endregion
}