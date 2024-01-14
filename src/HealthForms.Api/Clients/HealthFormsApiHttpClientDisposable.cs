using HealthForms.Api.Options;
using Microsoft.Extensions.Logging;

namespace HealthForms.Api.Clients;

public class HealthFormsApiHttpClientDisposable : HealthFormsApiHttpClient, IDisposable
{
    public HealthFormsApiHttpClientDisposable(HttpClient httpClient, HealthFormsApiOptions options, ILogger<HealthFormsApiHttpClient>? log = null) 
        : base(httpClient, Microsoft.Extensions.Options.Options.Create(options), log)
    {
    }

    public void Dispose()
    {
        HttpClient.Dispose();
    }
}