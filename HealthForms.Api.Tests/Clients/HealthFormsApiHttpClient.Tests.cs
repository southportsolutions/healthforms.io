using AutoFixture;
using HealthForms.Api.Clients;
using HealthForms.Api.Core.Models.SessionMember;
using HealthForms.Api.Errors;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit.Abstractions;

namespace HealthForms.Api.Tests.Clients;

public class HealthFormsApiHttpClientTests : UnitTestBase<HealthFormsApiHttpClient>
{
    private readonly ITestOutputHelper _testLogger;

    private string SessionId => Startup.TestOptions.SessionId;
    private string TenantId => Startup.TestOptions.TenantId;
    private string TenantToken => Startup.TestOptions.TenantToken;
    private string AttendeeId => Startup.TestOptions.AttendeeId;

    public HealthFormsApiHttpClientTests(ITestOutputHelper testLogger)
    {
        _testLogger = testLogger;
        Log = Fixture.Freeze<Mock<ILogger<HealthFormsApiHttpClient>>>();
        
        HttpClient = new HttpClient();
        Fixture.Register(() => HttpClient);

        var options = Microsoft.Extensions.Options.Options.Create(HealthFormsApiOptions);
        Fixture.Register(() => options);
        
        ClassUnderTest = Fixture.Create<HealthFormsApiHttpClient>();
    }

    public HttpClient HttpClient { get; set; }

    public Mock<ILogger<HealthFormsApiHttpClient>> Log { get; set; }

    #region Get Tenant Token

    [Fact]
    public async Task GetRequestUri()
    {
        //"customer:"
        var tenantId = Startup.TestOptions.TenantId;
        var redirectUrl = ClassUnderTest.GetRedirectUrl(tenantId);
        _testLogger.WriteLine("URI:{0}", redirectUrl.Uri);
        _testLogger.WriteLine("Code Verification: {0}", redirectUrl.CodeVerifier);

        var scope = HealthFormsApiOptions.Scopes.Replace("{{tenantId}}", tenantId);
        Assert.Contains(HealthFormsApiOptions.ClientId, redirectUrl.Uri, StringComparison.InvariantCultureIgnoreCase);
        Assert.Contains(Uri.EscapeDataString(HealthFormsApiOptions.RedirectUrl), redirectUrl.Uri, StringComparison.InvariantCultureIgnoreCase);
        Assert.Contains(Uri.EscapeDataString(scope), redirectUrl.Uri, StringComparison.InvariantCultureIgnoreCase);
    }
    [Fact]
    public async Task GetInitialToken_MissingCode()
    {
        var code = "";
        var codeVerifier = "Ww_Buz5Lmr_gpE0u8SeappD1IOJDdgS9hLH9TOX5zh_FpOU5_pxOxzS32ANtYlHhrosIqbYNTb8tw9UQ2FG9ag";
        var exception = await Assert.ThrowsAsync<ArgumentException>(() => ClassUnderTest.GetTenantToken(code, codeVerifier));
        Assert.Equal("code", exception.ParamName);
    }
    [Fact]
    public async Task GetInitialToken_CodeVerifier()
    {
        var code = "B776BD535431D57433C4C254EB80AC93902589503E0C929B289CC24CA4BBBE10-1";
        var codeVerifier = "";
        var exception = await Assert.ThrowsAsync<HealthFormsAuthException>(() => ClassUnderTest.GetTenantToken(code, codeVerifier));
        Assert.Equal("Unable to claim HealthForms.io code.", exception.Message);
    }

    //Used only when manually running tests. 
    //[Fact]
    //public async Task GetInitialToken()
    //{
    //    var code = "B776BD535431D57433C4C254EB80AC93902589503E0C929B289CC24CA4BBBE10-1";
    //    var codeVerifier = "Ww_Buz5Lmr_gpE0u8SeappD1IOJDdgS9hLH9TOX5zh_FpOU5_pxOxzS32ANtYlHhrosIqbYNTb8tw9UQ2FG9ag";
    //    var response = await ClassUnderTest.GetTenantToken(code, codeVerifier);
    //    Assert.NotNull(response);
    //}

    #endregion

    #region Get Access Token

    [Fact]
    public async Task GetAccessToken_MissingRefreshToken()
    {
        var tenantToken = "";
        var exception = await Assert.ThrowsAsync<HealthFormsException>(() => ClassUnderTest.GetAccessToken(tenantToken));
        Assert.Contains("Parameter is required (Parameter 'refresh_token'", exception.Message);
    }

    #endregion

    #region Get Session Member

    [Fact]
    public async Task GetSessionMembers_MissingTenantToken()
    {
        var exception = await Assert.ThrowsAsync<ArgumentNullException>(() => ClassUnderTest.GetSessionMembers("", TenantId, SessionId));
        Assert.Equal("tenantToken", exception.ParamName);
    }

    [Fact]
    public async Task GetSessionMembers_MissingTenantId()
    {
        var exception = await Assert.ThrowsAsync<ArgumentNullException>(() => ClassUnderTest.GetSessionMembers(TenantToken, "", SessionId));
        Assert.Equal("tenantId", exception.ParamName);
    }

    [Fact]
    public async Task GetSessionMembers_MissingSessionId()
    {
        var exception = await Assert.ThrowsAsync<ArgumentNullException>(() => ClassUnderTest.GetSessionMembers(TenantToken, TenantId, ""));
        Assert.Equal("sessionId", exception.ParamName);
    }

    [Fact]
    public async Task GetSessionMembers_InvalidTenantId()
    {
        var exception = await Assert.ThrowsAsync<ArgumentNullException>(() => ClassUnderTest.GetSessionMembers(TenantToken, TenantId, SessionId));
        Assert.Equal("tenantToken", exception.ParamName);
    }

    #endregion

    #region Add Session Member

    [Fact]
    public async Task AddSessionMember_MissingTenantToken()
    {
        var request = Fixture.Create<AddSessionMemberRequest>();
        var exception = await Assert.ThrowsAsync<ArgumentNullException>(() => ClassUnderTest.AddSessionMember("", TenantId, SessionId, request));
        Assert.Equal("tenantToken", exception.ParamName);
    }

    [Fact]
    public async Task AddSessionMember_MissingTenantId()
    {
        var request = Fixture.Create<AddSessionMemberRequest>();
        var exception = await Assert.ThrowsAsync<ArgumentNullException>(() => ClassUnderTest.AddSessionMember(TenantToken, "", SessionId, request));
        Assert.Equal("tenantId", exception.ParamName);
    }

    [Fact]
    public async Task AddSessionMember_MissingSessionId()
    {
        var request = Fixture.Create<AddSessionMemberRequest>();
        var exception = await Assert.ThrowsAsync<ArgumentNullException>(() => ClassUnderTest.AddSessionMember(TenantToken, TenantId, "", request));
        Assert.Equal("sessionId", exception.ParamName);
    }

    [Fact]
    public async Task AddSessionMember_MissingFirstName()
    {
        var request = Fixture.Create<AddSessionMemberRequest>();
        request.FirstName = "";
        var exception = await Assert.ThrowsAsync<HealthFormsException>(() => ClassUnderTest.AddSessionMember(TenantToken, TenantId, SessionId, request));
        Assert.Contains("3000", exception.Message);
        Assert.True(exception.Error.ValidationErrors.Any(c => c.Field == "FirstName"));
    }

    [Fact]
    public async Task AddSessionMember_MissingLastName()
    {
        var request = Fixture.Create<AddSessionMemberRequest>();
        request.LastName = "";
        var exception = await Assert.ThrowsAsync<HealthFormsException>(() => ClassUnderTest.AddSessionMember(TenantToken, TenantId, SessionId, request));
        Assert.Contains("3000", exception.Message);
        Assert.True(exception.Error.ValidationErrors.Any(c => c.Field == "LastName"));
    }

    [Fact]
    public async Task AddSessionMember_MissingEmail()
    {
        var request = Fixture.Create<AddSessionMemberRequest>();
        request.Email = "";
        var exception = await Assert.ThrowsAsync<HealthFormsException>(() => ClassUnderTest.AddSessionMember(TenantToken, TenantId, SessionId, request));
        Assert.Contains("3000", exception.Message);
        Assert.True(exception.Error.ValidationErrors.Any(c => c.Field == "Email"));
    }

    [Fact]
    public async Task AddSessionMember()
    {
        var request = Fixture.Create<AddSessionMemberRequest>();
        request.Email = "test1@southportsolutions.com";
        var response = await ClassUnderTest.AddSessionMember(TenantToken, TenantId, SessionId, request);
        Assert.NotNull(response);
        Assert.NotNull(response.AttendeeId);
        Assert.NotNull(response.MemberId);
        Assert.Equal(SessionId, response.SessionId);
        Assert.False(response.Accepted);
    }

    #endregion

    #region Add Session Member

    [Fact]
    public async Task AddSessionMembers_MissingTenantToken()
    {
        var request = Fixture.Create<List<AddSessionMemberRequest>>();
        var exception = await Assert.ThrowsAsync<ArgumentNullException>(() => ClassUnderTest.AddSessionMembers("", TenantId, SessionId, request));
        Assert.Equal("tenantToken", exception.ParamName);
    }

    [Fact]
    public async Task AddSessionMembers_MissingTenantId()
    {
        var request = Fixture.Create<List<AddSessionMemberRequest>>();
        var exception = await Assert.ThrowsAsync<ArgumentNullException>(() => ClassUnderTest.AddSessionMembers(TenantToken, "", SessionId, request));
        Assert.Equal("tenantId", exception.ParamName);
    }

    [Fact]
    public async Task AddSessionMembers_MissingSessionId()
    {
        var request = Fixture.Create<List<AddSessionMemberRequest>>();
        var exception = await Assert.ThrowsAsync<ArgumentNullException>(() => ClassUnderTest.AddSessionMembers(TenantToken, TenantId, "", request));
        Assert.Equal("sessionId", exception.ParamName);
    }

    [Fact]
    public async Task AddSessionMembers_MissingFirstName()
    {
        var request = Fixture.Create<List<AddSessionMemberRequest>>();
        request[0].FirstName = "";
        var exception = await Assert.ThrowsAsync<HealthFormsException>(() => ClassUnderTest.AddSessionMembers(TenantToken, TenantId, SessionId, request));
        Assert.Contains("3000", exception.Message);
        Assert.True(exception.Error.ValidationErrors.Any(c => c.Field == "FirstName"));
    }

    [Fact]
    public async Task AddSessionMembers_MissingLastName()
    {
        var request = Fixture.Create<List<AddSessionMemberRequest>>();
        request[0].LastName = "";
        var exception = await Assert.ThrowsAsync<HealthFormsException>(() => ClassUnderTest.AddSessionMembers(TenantToken, TenantId, SessionId, request));
        Assert.Contains("3000", exception.Message);
        Assert.True(exception.Error.ValidationErrors.Any(c => c.Field == "LastName"));
    }

    [Fact]
    public async Task AddSessionMembers_MissingEmail()
    {
        var request = Fixture.Create<List<AddSessionMemberRequest>>();
        request[0].Email = "";
        var exception = await Assert.ThrowsAsync<HealthFormsException>(() => ClassUnderTest.AddSessionMembers(TenantToken, TenantId, SessionId, request));
        Assert.Contains("3000", exception.Message);
        Assert.True(exception.Error.ValidationErrors.Any(c => c.Field == "Email"));
    }

    [Fact]
    public async Task AddSessionMembers_MoreThan100()
    {
        var request = new List<AddSessionMemberRequest>();
        do
        {
            request.AddRange(Fixture.Create<List<AddSessionMemberRequest>>());
        } while(request.Count < 101);

        var exception = await Assert.ThrowsAsync<HealthFormsException>(() => ClassUnderTest.AddSessionMembers(TenantToken, TenantId, SessionId, request));
        Assert.Equal("The maximum number of members that can be added at once is 100.", exception.Message);
    }

    [Fact]
    public async Task AddSessionMembers()
    {
        var request = Fixture.Create<List<AddSessionMemberRequest>>();
        foreach (var memberRequest in request)
        {
            memberRequest.SendInvitationOn = DateTime.UtcNow.AddDays(1);
            memberRequest.Email = "test1@southportsolutions.com";
        }
        var response = await ClassUnderTest.AddSessionMembers(TenantToken, TenantId, SessionId, request);
        Assert.NotNull(response);
        Assert.Empty(response.Errors);
        Assert.Equal(request.Count, response.AddedMembers.Count());
        foreach (var addedMember in response.AddedMembers)
        {
            Assert.NotNull(addedMember.AttendeeId);
            Assert.NotNull(addedMember.MemberId);
            Assert.NotNull(addedMember.InvitationSendOn);
            Assert.Null(addedMember.InvitationSentOn);
            Assert.Equal(SessionId, addedMember.SessionId);
            Assert.False(addedMember.Accepted);
        }
    }

    #endregion
}