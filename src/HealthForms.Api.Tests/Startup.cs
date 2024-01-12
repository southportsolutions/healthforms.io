using HealthForms.Api.Options;
using HealthForms.Api.Tests.Options;
using Microsoft.Extensions.Configuration;

namespace HealthForms.Api.Tests;

public static class Startup
{
    public static HealthFormsApiOptions Options => TestOptions;
    public static HealthFormsApiTestOptions TestOptions { get; private set; } = new();

    public static HealthFormsApiOptions GetOptions()
    {
        var configurationBuilder = new ConfigurationBuilder()
                    
            .AddJsonFile(Path.Combine((new DirectoryInfo(Environment.CurrentDirectory).Parent?.Parent?.Parent)?.ToString() ?? string.Empty, "appsettings.json"), true)
            .AddEnvironmentVariables();
        var config = configurationBuilder.Build();
        TestOptions = new HealthFormsApiTestOptions();
        var section = config.GetSection(HealthFormsApiOptions.Key);
        section.Bind(Options);

        if (string.IsNullOrWhiteSpace(Options.ClientId))
        {
            TestOptions.ClientId = Environment.GetEnvironmentVariable("HFPCLIENTID") ?? throw new InvalidOperationException();
            TestOptions.ClientSecret = Environment.GetEnvironmentVariable("HFPCLIENTSECRET") ?? throw new InvalidOperationException();
            TestOptions.TenantToken = Environment.GetEnvironmentVariable("HFTENANTTOKEN") ?? throw new InvalidOperationException();
            TestOptions.TenantId = Environment.GetEnvironmentVariable("HFTENANTID") ?? throw new InvalidOperationException();
            TestOptions.SessionId = Environment.GetEnvironmentVariable("HFSESSIONID") ?? throw new InvalidOperationException();
        }

        if (string.IsNullOrEmpty(Options.ClientId))
        {
            throw new Exception("Unable to get the HealthForms Client Info.");
        }

        return Options;

    }
}