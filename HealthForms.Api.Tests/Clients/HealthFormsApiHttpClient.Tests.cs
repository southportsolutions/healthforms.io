using AutoFixture;
using HealthForms.Api.Clients;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace HealthForms.Api.Tests.Clients;

public class HealthFormsApiHttpClientTests : UnitTestBase<HealthFormsApiHttpClient>
{
    public HealthFormsApiHttpClientTests()
    {
        Log = Fixture.Freeze<Mock<ILogger<HealthFormsApiHttpClient>>>();
        Cache =  Fixture.Freeze<Mock<MemoryCache>>();
        
        HttpClient = new HttpClient();
        Fixture.Register(() => HttpClient);

        var options = Microsoft.Extensions.Options.Options.Create(HealthFormsApiOptions);
        Fixture.Register(() => options);
        
        ClassUnderTest = Fixture.Create<HealthFormsApiHttpClient>();
    }

    public HttpClient HttpClient { get; set; }
    public Mock<MemoryCache> Cache { get; set; }

    public Mock<ILogger<HealthFormsApiHttpClient>> Log { get; set; }

    [Fact]
    public async Task GetRequestUri()
    {
        var redirectUrl = ClassUnderTest.GetRedirectUrl();
        Assert.Contains(HealthFormsApiOptions.ClientId, redirectUrl.Uri);
        Assert.Contains(HealthFormsApiOptions.RedirectUrl, redirectUrl.Uri);
    }
    [Fact]
    public async Task GetToken()
    {
        //var httpClient = new HttpClient();
        //var client = new HealthFormsApiHttpClient(httpClient, Microsoft.Extensions.Options.Options.Create(HealthFormsApiOptions), new MemoryCache(), new NullLogger<HealthFormsApiHttpClient>());
        var response = await ClassUnderTest.GetTenantToken("6EF8CE38A1C83566CFF1D9D8AEA3F26C473BE7322433C621193DF87C47453350-1", "WxY78-mP3KV2tc7NQVu4126Pn-IEWxsf4HF_ppdip9bWzFnjDCILQe0-JSs4Bpy0JModsKNnjO6zW-tckJ3WTg");
        //Assert.NotNull(response);
        //Assert.NotNull(response.AccessToken);
        //Assert.NotNull(response.TokenType);
        //Assert.NotNull(response.ExpiresIn);
        //Assert.True(response.ExpiresOn > DateTime.Now);
    }
}