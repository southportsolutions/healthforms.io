namespace HealthForms.Api.Core.Models.Auth.Interface;

public interface IAuthResponse
{
    string? IdToken { get; set; }
    string? AccessToken { get; set; }
    string? TokenType { get; set; }
    int? ExpiresIn { get; set; }
    DateTime ExpiresOn { get; set; }
}