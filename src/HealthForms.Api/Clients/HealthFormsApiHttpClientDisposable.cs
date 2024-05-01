using HealthForms.Api.Options;
using Microsoft.Extensions.Logging;

namespace HealthForms.Api.Clients;

public class HealthFormsApiHttpClientDisposable : HealthFormsApiHttpClient, IDisposable
{
    public HealthFormsApiHttpClientDisposable(HttpClient httpClient, HealthFormsApiOptions options, ILogger<HealthFormsApiHttpClient>? log) 
        : base(httpClient, Microsoft.Extensions.Options.Options.Create(options), log)
    {
    }
    public HealthFormsApiHttpClientDisposable(HttpClient httpClient, HealthFormsApiOptions options) 
        : base(httpClient, Microsoft.Extensions.Options.Options.Create(options))
    {
    }

    public void Dispose()
    {
        HttpClient.Dispose();
    }
}