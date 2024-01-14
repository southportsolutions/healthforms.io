namespace HealthForms.Api.Core.Models.Auth.Interface;

public interface IAuthRedirect
{
    string CodeVerifier { get; set; }
    string Uri { get; set; }
}