using IdentityModel.Client;

namespace HealthForms.Api.Clients;

public class AuthResponse
{
    public AuthResponse(TokenResponse response)
    {
        IdToken = response.IdentityToken;
        AccessToken = response.AccessToken;
        TokenType = response.TokenType;
        ExpiresIn = response.ExpiresIn;
        ExpiresOn = DateTime.UtcNow.AddSeconds(response.ExpiresIn - 5);
    }

    public string? IdToken { get; set; }
    public string? AccessToken { get; set; }
    public string? TokenType { get; set; }
    public int? ExpiresIn { get; set; }
    public DateTime ExpiresOn { get; set; }
}