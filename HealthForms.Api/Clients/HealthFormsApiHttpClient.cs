using HealthForms.Api.Options;
using Microsoft.Extensions.Options;
using System.Text;
using System.Text.Json;
using HealthForms.Api.Errors;
using Microsoft.Extensions.Logging;

namespace HealthForms.Api.Clients;

public class HealthFormsApiHttpClient
{
    private ILogger<HealthFormsApiHttpClient> Log { get; }
    private readonly HttpClient _httpClient;
    private readonly HealthFormsApiOptions _options;
    private AuthResponse? _accessToken;

    public HealthFormsApiHttpClient(HttpClient httpClient, IOptions<HealthFormsApiOptions> options, ILogger<HealthFormsApiHttpClient> log)
    {
        Log = log;
        _httpClient = httpClient;
        _options = options.Value;
    }

    #region Access Token

    public async Task<AuthResponse> GetAccessToken()
    {
        if (_accessToken!=null && _accessToken.ExpiresOn >= DateTime.UtcNow) return _accessToken;

        var content = new StringContent($"grant_type=client_credentials&client_id={_options.ClientId}&client_secret={_options.ClientSecret}&scope={_options.Scopes}", Encoding.UTF8, "application/x-www-form-urlencoded");
        try
        {
            var response = await _httpClient.PostAsync($"{_options.HostAddressAuth}connect/token", content);
            if (!response.IsSuccessStatusCode) throw new HealthFormsException($"Error getting the admin token. Status Code: {response.StatusCode} Address: {response.RequestMessage.RequestUri} Message: {await response.Content.ReadAsStringAsync()}");

            var tokenString = await response.Content.ReadAsStringAsync();
            var token = JsonSerializer.Deserialize<AuthResponse>(tokenString);
            _accessToken = token ?? throw new HealthFormsException($"The admin token response was corrupt. Status Code: {response.StatusCode} Address: {response.RequestMessage.RequestUri}");
                
            _accessToken.ExpiresOn = DateTime.UtcNow.AddSeconds(_accessToken.ExpiresIn - 5 ?? 0);
            _accessToken = token;
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