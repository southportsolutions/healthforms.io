namespace HealthForms.Api.Core.Models;

public class AuthRedirect
{
    public string CodeVerifier { get; set; } = null!;
    public string Uri { get; set; } = null!;
}