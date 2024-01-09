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
        //"customer:"
        var redirectUrl = ClassUnderTest.GetRedirectUrl("oALPOYAnIQ");
        Assert.Contains(HealthFormsApiOptions.ClientId, redirectUrl.Uri);
        Assert.Contains(HealthFormsApiOptions.RedirectUrl, redirectUrl.Uri);
    }
    [Fact]
    public async Task GetToken()
    {
        //var httpClient = new HttpClient();
        //var client = new HealthFormsApiHttpClient(httpClient, Microsoft.Extensions.Options.Options.Create(HealthFormsApiOptions), new MemoryCache(), new NullLogger<HealthFormsApiHttpClient>());
        var response = await ClassUnderTest.GetTenantToken("B776BD535431D57433C4C254EB80AC93902589503E0C929B289CC24CA4BBBE10-1", "Ww_Buz5Lmr_gpE0u8SeappD1IOJDdgS9hLH9TOX5zh_FpOU5_pxOxzS32ANtYlHhrosIqbYNTb8tw9UQ2FG9ag");
        Assert.NotNull(response);
        //Assert.NotNull(response.AccessToken);
        //Assert.NotNull(response.TokenType);
        //Assert.NotNull(response.ExpiresIn);
        //Assert.True(response.ExpiresOn > DateTime.Now);
    }
}