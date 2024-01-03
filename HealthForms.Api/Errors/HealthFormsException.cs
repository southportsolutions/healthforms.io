using HealthForms.Api.Clients;
using IdentityModel.Client;

namespace HealthForms.Api.Errors;

public class HealthFormsException : Exception
{
    public HealthFormsException(string message) : base(message)
    {
    }
}
public class HealthFormsAuthException : HealthFormsException
{
    public AuthResponse Response { get; }

    public HealthFormsAuthException(string message, TokenResponse response) : base(message)
    {
        Response = response;
    }
}