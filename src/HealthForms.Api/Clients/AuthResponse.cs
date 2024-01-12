using HealthForms.Api.Core.Models.Auth.Interface;
using HealthForms.Api.Errors;
using IdentityModel.Client;

namespace HealthForms.Api.Clients;

public class AuthResponse : IAuthResponse
{
    public AuthResponse(TokenResponse response)
    {
        IdToken = response.IdentityToken;
        AccessToken = response.AccessToken ?? throw new HealthFormsException("The access token cannot be null.");
        TokenType = response.TokenType;
        ExpiresIn = response.ExpiresIn;
        ExpiresOn = DateTime.UtcNow.AddSeconds(response.ExpiresIn - 5);
    }

    public string? IdToken { get; set; }
    public string AccessToken { get; set; }
    public string? TokenType { get; set; }
    public int? ExpiresIn { get; set; }
    public DateTime ExpiresOn { get; set; }
}