using HealthForms.Api.Core.Models.Errors;
using IdentityModel.Client;

namespace HealthForms.Api.Errors;

public class HealthFormsException : Exception
{
    public HealthFormsErrorResponse? Error { get; }

    public HealthFormsException(string message) : base(message)
    {
    }
    public HealthFormsException(HealthFormsErrorResponse error) : base(error.Message)
    {
        Error = error;
    }
}
public class HealthFormsAuthException : HealthFormsException
{
    public TokenResponse Response { get; }

    public HealthFormsAuthException(string message, TokenResponse response) : base(message)
    {
        Response = response;
    }
}