using HealthForms.Api.Options;
using HealthForms.Api.Tests.Options;
using Microsoft.Extensions.Configuration;

namespace HealthForms.Api.Tests;

public static class Startup
{
    public static HealthFormsApiOptions Options { get; private set; } = new HealthFormsApiTestOptions();

    public static HealthFormsApiOptions GetOptions()
    {
        var configurationBuilder = new ConfigurationBuilder()
                    
            .AddJsonFile(Path.Combine((new DirectoryInfo(Environment.CurrentDirectory).Parent?.Parent?.Parent)?.ToString() ?? string.Empty, "appsettings.json"), true)
            .AddEnvironmentVariables();
        var config = configurationBuilder.Build();
        Options = new HealthFormsApiTestOptions();
        var section = config.GetSection(HealthFormsApiOptions.Key);
        section.Bind(Options);

        if (string.IsNullOrWhiteSpace(Options.ClientId))
        {
            Options.ClientId = Environment.GetEnvironmentVariable("HFPCLIENTID") ?? throw new InvalidOperationException();
            Options.ClientSecret = Environment.GetEnvironmentVariable("HFPCLIENTSECRET") ?? throw new InvalidOperationException();
        }

        if (string.IsNullOrEmpty(Options.ClientId))
        {
            throw new Exception("Unable to get the HealthForms Client Info.");
        }

        return Options;

    }
}