using HealthForms.Api.Core.Models.Auth.Interface;

namespace HealthForms.Api.Core.Models.Auth;

public class AuthRedirect : IAuthRedirect
{
    public string CodeVerifier { get; set; } = null!;
    public string Uri { get; set; } = null!;
}