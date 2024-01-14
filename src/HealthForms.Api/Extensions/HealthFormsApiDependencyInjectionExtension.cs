using HealthForms.Api.Clients;
using HealthForms.Api.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Contrib.WaitAndRetry;
using Polly.Extensions.Http;

namespace HealthForms.Api.Extensions;

public static class HealthFormsApiDependencyInjectionExtension
{
    public static IServiceCollection AddHealthForms(this IServiceCollection services, IConfigurationSection optionsSection) 
    {
        services.Configure<HealthFormsApiOptions>(optionsSection);
        services.AddHttpClient<HealthFormsApiHttpClient>().AddPolicyHandler(GetRetryPolicy());
        
        return services;
    }

    static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy(int medianFirstRetryDelay = 35, int retryCount = 7)
    {
        var delay = Backoff.DecorrelatedJitterBackoffV2(medianFirstRetryDelay: TimeSpan.FromMilliseconds(medianFirstRetryDelay), retryCount: retryCount);
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .WaitAndRetryAsync(delay);
    }
}