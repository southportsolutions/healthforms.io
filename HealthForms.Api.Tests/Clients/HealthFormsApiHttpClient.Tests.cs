using HealthForms.Api.Clients;
using Microsoft.Extensions.Logging.Abstractions;

namespace HealthForms.Api.Tests.Clients;

public class HealthFormsApiHttpClientTests : UnitTestBase
{
    [Fact]
    public async Task GetToken()
    {
        var httpClient = new HttpClient();
        var client = new HealthFormsApiHttpClient(httpClient, Microsoft.Extensions.Options.Options.Create(HealthFormsApiOptions), new NullLogger<HealthFormsApiHttpClient>());
        var response = await client.GetAccessToken();
        Assert.NotNull(response);
        Assert.NotNull(response.AccessToken);
        Assert.NotNull(response.TokenType);
        Assert.NotNull(response.ExpiresIn);
        Assert.True(response.ExpiresOn > DateTime.Now);
    }
}