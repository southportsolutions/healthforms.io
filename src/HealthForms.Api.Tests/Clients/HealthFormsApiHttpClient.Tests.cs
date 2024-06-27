using AutoFixture;
using HealthForms.Api.Clients;
using HealthForms.Api.Core.Models.SessionMember;
using HealthForms.Api.Core.Models.Webhooks;
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
        var code = "75BF86CC3D65277EB08B14F5693A2687DF4F6570FFA7AC5A5C303E5102E71154-1";
        var codeVerifier = "";
        var exception = await Assert.ThrowsAsync<HealthFormsAuthException>(() => ClassUnderTest.GetTenantToken(code, codeVerifier));
        Assert.Equal("Unable to claim HealthForms.io code.", exception.Message);
    }

    //Used only when manually running tests. 
    //[Fact]
    //public async Task GetInitialToken()
    //{
    //    var code = "75BF86CC3D65277EB08B14F5693A2687DF4F6570FFA7AC5A5C303E5102E71154-1";
    //    var codeVerifier = "Eui3Dhpv9ctQAEn9_sOOzbzHXpaHDNXxLOHrrG2t1GGQ_C9p_-cYGGSJnL94DaZcRk6bTfZ6QXK0XMArdEiCbA";
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

    #region Get Session Members

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
        var exception = await ClassUnderTest.GetSessionMembers(TenantToken, "123456789a", SessionId);
        Assert.Contains("4003", exception.ErrorMessage);
    }

    [Fact]
    public async Task GetSessionMembers()
    {
        var response = await ClassUnderTest.GetSessionMembers(TenantToken, TenantId, SessionId);
        Assert.NotNull(response);
        Assert.NotEmpty(response.Data.Data);
        Assert.NotNull(response.Data.NextUri);


        response = await ClassUnderTest.GetSessionMembers(TenantToken, response.Data.NextUri);
        Assert.NotNull(response);
        Assert.NotEmpty(response.Data.Data);


    }

    [Fact]
    public async Task GetSessionMembers_ExtraPage()
    {
        var response = await ClassUnderTest.GetSessionMembers(TenantToken, TenantId, SessionId, 100);
        Assert.NotNull(response);
        Assert.Empty(response.Data.Data);
    }

    #endregion
    
    #region Find Session Members

    [Fact]
    public async Task SearchSessionMembers_MissingTenantToken()
    {
        var request = new SessionMemberSearchRequest();
        var exception = await Assert.ThrowsAsync<ArgumentNullException>(() => ClassUnderTest.SearchSessionMember("", TenantId, SessionId, request));
        Assert.Equal("tenantToken", exception.ParamName);
    }

    [Fact]
    public async Task SearchSessionMembers_MissingTenantId()
    {
        var request = new SessionMemberSearchRequest();
        var exception = await Assert.ThrowsAsync<ArgumentNullException>(() => ClassUnderTest.SearchSessionMember(TenantToken, "", SessionId, request));
        Assert.Equal("tenantId", exception.ParamName);
    }

    [Fact]
    public async Task SearchSessionMembers_MissingSessionId()
    {
        var request = new SessionMemberSearchRequest();
        var exception = await Assert.ThrowsAsync<ArgumentNullException>(() => ClassUnderTest.SearchSessionMember(TenantToken, TenantId, "", request));
        Assert.Equal("sessionId", exception.ParamName);
    }

    [Fact]
    public async Task SearchSessionMembers_InvalidTenantId()
    {
        var request = new SessionMemberSearchRequest();
        var exception = await ClassUnderTest.SearchSessionMember(TenantToken, "123456789a", SessionId, request);
        Assert.Contains("4003", exception.ErrorMessage);
    }

    [Fact]
    public async Task SearchSessionMembers()
    {
        var request = new SessionMemberSearchRequest(){ExternalAttendeeId = "123456789a", ExternalMemberId = "123456789b"};
        var response = await ClassUnderTest.SearchSessionMember(TenantToken, TenantId, SessionId, request);
        Assert.NotNull(response);
        Assert.Equal(SessionMemberSearchResult.None, response.Data.Result);
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
        var exception = await ClassUnderTest.AddSessionMember(TenantToken, TenantId, SessionId, request);
        Assert.Contains("3000", exception.ErrorMessage);
        Assert.Contains(exception.Error.ValidationErrors, c => c.Field == "FirstName");
    }

    [Fact]
    public async Task AddSessionMember_MissingLastName()
    {
        var request = Fixture.Create<AddSessionMemberRequest>();
        request.LastName = "";
        var exception = await ClassUnderTest.AddSessionMember(TenantToken, TenantId, SessionId, request);
        Assert.Contains("3000", exception.ErrorMessage);
        Assert.Contains(exception.Error.ValidationErrors, c => c.Field == "LastName");
    }

    [Fact]
    public async Task AddSessionMember_MissingEmail()
    {
        var request = Fixture.Create<AddSessionMemberRequest>();
        request.Email = "";
        var exception = await ClassUnderTest.AddSessionMember(TenantToken, TenantId, SessionId, request);
        Assert.Contains("3000", exception.ErrorMessage);
        Assert.Contains(exception.Error.ValidationErrors, c => c.Field == "Email");
    }

    [Fact]
    public async Task AddSessionMember()
    {
        var request = Fixture.Create<AddSessionMemberRequest>();
        request.Email = "test1@southportsolutions.com";
        var response = await ClassUnderTest.AddSessionMember(TenantToken, TenantId, SessionId, request);
        Assert.NotNull(response);
        Assert.NotNull(response.Data.Id);
        Assert.NotNull(response.Data.Status);
        Assert.True(response.Data.Forms.Any());
        Assert.Equal(SessionId, response.Data.SessionId);
        Assert.False(response.Data.InvitationAccepted);
    }

    #endregion

    #region Add Session Members  (Bulk)

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
        var exception = await ClassUnderTest.AddSessionMembers(TenantToken, TenantId, SessionId, request);
        Assert.Contains("3000", exception.ErrorMessage);
        Assert.Contains(exception.Error.ValidationErrors, c => c.Field == "FirstName");
    }

    [Fact]
    public async Task AddSessionMembers_MissingLastName()
    {
        var request = Fixture.Create<List<AddSessionMemberRequest>>();
        request[0].LastName = "";
        var exception = await ClassUnderTest.AddSessionMembers(TenantToken, TenantId, SessionId, request);
        Assert.Contains("3000", exception.ErrorMessage);
        Assert.Contains(exception.Error.ValidationErrors, c => c.Field == "LastName");
    }

    [Fact]
    public async Task AddSessionMembers_MissingEmail()
    {
        var request = Fixture.Create<List<AddSessionMemberRequest>>();
        request[0].Email = "";
        var response = await ClassUnderTest.AddSessionMembers(TenantToken, TenantId, SessionId, request);
        Assert.Contains("3000", response.Error.Message);
        Assert.Contains(response.Error.ValidationErrors, c => c.Field == "Email");
    }

    [Fact]
    public async Task AddSessionMembers_MoreThan1000()
    {
        var request = new List<AddSessionMemberRequest>();
        do
        {
            request.AddRange(Fixture.Create<List<AddSessionMemberRequest>>());
        } while(request.Count < 1001);

        var exception = await Assert.ThrowsAsync<HealthFormsException>(() => ClassUnderTest.AddSessionMembers(TenantToken, TenantId, SessionId, request));
        Assert.Equal("The maximum number of members that can be added at once is 1000.", exception.Message);
    }

    [Fact]
    public async Task AddSessionMembers()
    {
        var request = Fixture.Create<List<AddSessionMemberRequest>>();

        var i = 1;
        foreach (var memberRequest in request)
        {
            memberRequest.FirstName = $"John_{i}";
            memberRequest.LastName = $"Doe_{i}";
            memberRequest.Group = null;
            memberRequest.Phone = "555-555-5555";
            memberRequest.ExternalAttendeeId = Guid.NewGuid().ToString("N");
            memberRequest.ExternalMemberId = Guid.NewGuid().ToString("N");
            memberRequest.SendInvitationOn = DateTime.UtcNow.AddDays(100);
            memberRequest.Email = "test1@southportsolutions.com";
            i++;
        }
        var response = await ClassUnderTest.AddSessionMembers(TenantToken, TenantId, SessionId, request);
        Assert.NotNull(response);
        Assert.NotNull(response.Data.Id);

        AddSessionMemberBulkResponse status;
        do
        {
            status = (await ClassUnderTest.GetAddSessionMembersStatus(TenantToken, TenantId, SessionId, response.Data.Id)).Data;
            await Task.Delay(1000);

        } while(status.PercentComplete < 1);

        Assert.Equal("Finished importing rows", status.StatusMessage);
        Assert.NotNull(status.StatusMessage);
        Assert.Equal(3, status.CountAdded);
        Assert.Equal(3, status.CountTotal);
        Assert.Equal(0, status.CountSkipped);
        Assert.False(status.HasErrors);
        Assert.Equal(1, status.PercentComplete);
        Assert.NotNull(status.Results);
        Assert.NotEmpty(status.Results);
        Assert.Equal(3, status.Results.Count);
    }

    #endregion

    #region Update Session Member

    [Fact]
    public async Task UpdateSessionMember_MissingTenantToken()
    {
        var request = Fixture.Create<UpdateSessionMemberRequest>();
        var exception = await Assert.ThrowsAsync<ArgumentNullException>(() => ClassUnderTest.UpdateSessionMember("", TenantId, SessionId, request));
        Assert.Equal("tenantToken", exception.ParamName);
    }

    [Fact]
    public async Task UpdateSessionMember_MissingTenantId()
    {
        var request = Fixture.Create<UpdateSessionMemberRequest>();
        var exception = await Assert.ThrowsAsync<ArgumentNullException>(() => ClassUnderTest.UpdateSessionMember(TenantToken, "", SessionId, request));
        Assert.Equal("tenantId", exception.ParamName);
    }

    [Fact]
    public async Task UpdateSessionMember_MissingSessionId()
    {
        var request = Fixture.Create<UpdateSessionMemberRequest>();
        var exception = await Assert.ThrowsAsync<ArgumentNullException>(() => ClassUnderTest.UpdateSessionMember(TenantToken, TenantId, "", request));
        Assert.Equal("sessionId", exception.ParamName);
    }

    [Fact]
    public async Task UpdateSessionMember_MissingFirstName()
    {
        var addRequest = Fixture.Create<AddSessionMemberRequest>();
        addRequest.ExternalAttendeeId = Guid.NewGuid().ToString("N");
        addRequest.ExternalMemberId = null;
        addRequest.Email = "test1@southportsolutions.com";
        var addResponse = await ClassUnderTest.AddSessionMember(TenantToken, TenantId, SessionId, addRequest);

        var request = new UpdateSessionMemberRequest()
        {
            SessionMemberId = addResponse.Data.Id,
            Email = "test1@southportsolutions.com",
            FirstName = "",
            LastName = "Doe",
            Group = "Group 1",
            Phone = "555-555-5555",
            ExternalAttendeeId = addRequest.ExternalAttendeeId,
            ExternalMemberId = addRequest.ExternalMemberId,
            SendInvitationOn = null
        };

        request.FirstName = "";
        var exception = await ClassUnderTest.UpdateSessionMember(TenantToken, TenantId, SessionId, request);
        Assert.Contains("3000", exception.ErrorMessage);
        Assert.Contains(exception.Error.ValidationErrors, c => c.Field == "FirstName");
    }

    [Fact]
    public async Task UpdateSessionMember_MissingLastName()
    {
        var addRequest = Fixture.Create<AddSessionMemberRequest>();
        addRequest.ExternalAttendeeId = Guid.NewGuid().ToString("N");
        addRequest.ExternalMemberId = null;
        addRequest.Email = "test1@southportsolutions.com";
        var addResponse = await ClassUnderTest.AddSessionMember(TenantToken, TenantId, SessionId, addRequest);

        var request = new UpdateSessionMemberRequest()
        {
            SessionMemberId = addResponse.Data.Id,
            Email = "test1@southportsolutions.com",
            FirstName = "John",
            LastName = "",
            Group = "Group 1",
            Phone = "555-555-5555",
            ExternalAttendeeId = addRequest.ExternalAttendeeId,
            ExternalMemberId = addRequest.ExternalMemberId,
            SendInvitationOn = null
        };

        var exception = await ClassUnderTest.UpdateSessionMember(TenantToken, TenantId, SessionId, request);
        Assert.Contains("3000", exception.ErrorMessage);
        Assert.Contains(exception.Error.ValidationErrors, c => c.Field == "LastName");
    }

    [Fact]
    public async Task UpdateSessionMember_MissingEmail()
    {
        var addRequest = Fixture.Create<AddSessionMemberRequest>();
        addRequest.ExternalAttendeeId = Guid.NewGuid().ToString("N");
        addRequest.ExternalMemberId = null;
        addRequest.Email = "test1@southportsolutions.com";
        var addResponse = await ClassUnderTest.AddSessionMember(TenantToken, TenantId, SessionId, addRequest);

        var request = new UpdateSessionMemberRequest()
        {
            SessionMemberId = addResponse.Data.Id,
            Email = "",
            FirstName = "John",
            LastName = "Doe",
            Group = "Group 1",
            Phone = "555-555-5555",
            ExternalAttendeeId = addRequest.ExternalAttendeeId,
            ExternalMemberId = addRequest.ExternalMemberId,
            SendInvitationOn = null
        };

        var exception = await ClassUnderTest.UpdateSessionMember(TenantToken, TenantId, SessionId, request);
        Assert.Contains("3000", exception.ErrorMessage);
        Assert.Contains(exception.Error.ValidationErrors, c => c.Field == "Email");
    }

    [Fact]
    public async Task UpdateSessionMember()
    {
        var addRequest = Fixture.Create<AddSessionMemberRequest>();
        addRequest.ExternalAttendeeId = Guid.NewGuid().ToString("N");
        addRequest.ExternalMemberId = null;
        addRequest.Email = "test1@southportsolutions.com";
        var addResponse = await ClassUnderTest.AddSessionMember(TenantToken, TenantId, SessionId, addRequest);

        var request = new UpdateSessionMemberRequest()
        {
            SessionMemberId = addResponse.Data.Id,
            Email = "test2@southportsolutions.com",
            FirstName = "John",
            LastName = "Doe",
            Group = "Group 1",
            Phone = "555-555-5555",
            ExternalAttendeeId = addRequest.ExternalAttendeeId,
            ExternalMemberId = addRequest.ExternalMemberId,
            SendInvitationOn = null
        };

        var response = await ClassUnderTest.UpdateSessionMember(TenantToken, TenantId, SessionId, request);
        Assert.Equal(request.FirstName, response.Data.FirstName);
        Assert.Equal(request.LastName, response.Data.LastName);
        Assert.Equal(request.ExternalAttendeeId, response.Data.ExternalAttendeeId);
        Assert.Equal(request.ExternalMemberId, response.Data.ExternalMemberId);
        Assert.False(response.Data.IsComplete);
        Assert.True(response.Data.Forms.Any());
    }

    #endregion

    #region Delete Session Members

    [Fact]
    public async Task DeleteSessionMember_MissingTenantToken()
    {
        var exception = await Assert.ThrowsAsync<ArgumentNullException>(() => ClassUnderTest.DeleteSessionMember("", TenantId, SessionId, Guid.NewGuid().ToString("N")));
        Assert.Equal("tenantToken", exception.ParamName);
    }

    [Fact]
    public async Task DeleteSessionMember_MissingTenantId()
    {
        var exception = await Assert.ThrowsAsync<ArgumentNullException>(() => ClassUnderTest.DeleteSessionMember(TenantToken, "", SessionId, Guid.NewGuid().ToString("N")));
        Assert.Equal("tenantId", exception.ParamName);
    }

    [Fact]
    public async Task DeleteSessionMember_MissingSessionId()
    {
        var exception = await Assert.ThrowsAsync<ArgumentNullException>(() => ClassUnderTest.DeleteSessionMember(TenantToken, TenantId, "", Guid.NewGuid().ToString("N")));
        Assert.Equal("sessionId", exception.ParamName);
    }

    [Fact]
    public async Task DeleteSessionMember_MissingMissingSessionMemberId()
    {
        var exception = await Assert.ThrowsAsync<ArgumentNullException>(() => ClassUnderTest.DeleteSessionMember(TenantToken, TenantId, SessionId, ""));
        Assert.Equal("sessionMemberId", exception.ParamName);
    }

    [Fact]
    public async Task DeleteSessionMember_InvalidId()
    {
        await ClassUnderTest.DeleteSessionMember(TenantToken, TenantId, SessionId, "123456789a");
    }

    [Fact]
    public async Task DeleteSessionMember()
    {
        var request = Fixture.Create<AddSessionMemberRequest>();
        request.Email = "test1@southportsolutions.com";
        var response = await ClassUnderTest.AddSessionMember(TenantToken, TenantId, SessionId, request);

        var apiResponse = await ClassUnderTest.DeleteSessionMember(TenantToken, TenantId, SessionId, response.Data.Id);
        Assert.True(apiResponse.IsSuccess);
    }

    [Fact]
    public async Task DeleteSessionMemberByExternalAttendeeId_MissingTenantToken()
    {
        var exception = await Assert.ThrowsAsync<ArgumentNullException>(() => ClassUnderTest.DeleteSessionMemberByExternalAttendeeId("", TenantId, SessionId, Guid.NewGuid().ToString("N")));
        Assert.Equal("tenantToken", exception.ParamName);
    }

    [Fact]
    public async Task DeleteSessionMemberByExternalAttendeeId_MissingTenantId()
    {
        var exception = await Assert.ThrowsAsync<ArgumentNullException>(() => ClassUnderTest.DeleteSessionMemberByExternalAttendeeId(TenantToken, "", SessionId, Guid.NewGuid().ToString("N")));
        Assert.Equal("tenantId", exception.ParamName);
    }

    [Fact]
    public async Task DeleteSessionMemberByExternalAttendeeId_MissingSessionId()
    {
        var exception = await Assert.ThrowsAsync<ArgumentNullException>(() => ClassUnderTest.DeleteSessionMemberByExternalAttendeeId(TenantToken, TenantId, "", Guid.NewGuid().ToString("N")));
        Assert.Equal("sessionId", exception.ParamName);
    }

    [Fact]
    public async Task DeleteSessionMemberByExternalAttendeeId_MissingMissingId()
    {
        var exception = await Assert.ThrowsAsync<ArgumentNullException>(() => ClassUnderTest.DeleteSessionMemberByExternalAttendeeId(TenantToken, TenantId, SessionId, ""));
        Assert.Equal("externalAttendeeId", exception.ParamName);
    }

    [Fact]
    public async Task DeleteSessionMemberByExternalAttendeeId_InvalidId()
    {
        await ClassUnderTest.DeleteSessionMemberByExternalAttendeeId(TenantToken, TenantId, SessionId, "123456789a");
    }

    [Fact]
    public async Task DeleteSessionMemberByExternalAttendeeId()
    {
        var request = Fixture.Create<AddSessionMemberRequest>();
        request.Email = "test1@southportsolutions.com";
        await ClassUnderTest.AddSessionMember(TenantToken, TenantId, SessionId, request);

        var isSuccessful = await ClassUnderTest.DeleteSessionMemberByExternalAttendeeId(TenantToken, TenantId, SessionId, request.ExternalAttendeeId);
        Assert.True(isSuccessful.IsSuccess);
    }

    [Fact]
    public async Task DeleteSessionMemberByExternalId_MissingTenantToken()
    {
        var exception = await Assert.ThrowsAsync<ArgumentNullException>(() => ClassUnderTest.DeleteSessionMemberByExternalId("", TenantId, SessionId, Guid.NewGuid().ToString("N")));
        Assert.Equal("tenantToken", exception.ParamName);
    }

    [Fact]
    public async Task DeleteSessionMemberByExternalId_MissingTenantId()
    {
        var exception = await Assert.ThrowsAsync<ArgumentNullException>(() => ClassUnderTest.DeleteSessionMemberByExternalId(TenantToken, "", SessionId, Guid.NewGuid().ToString("N")));
        Assert.Equal("tenantId", exception.ParamName);
    }

    [Fact]
    public async Task DeleteSessionMemberByExternalId_MissingSessionId()
    {
        var exception = await Assert.ThrowsAsync<ArgumentNullException>(() => ClassUnderTest.DeleteSessionMemberByExternalId(TenantToken, TenantId, "", Guid.NewGuid().ToString("N")));
        Assert.Equal("sessionId", exception.ParamName);
    }

    [Fact]
    public async Task DeleteSessionMemberByExternalId_MissingId()
    {
        var exception = await Assert.ThrowsAsync<ArgumentNullException>(() => ClassUnderTest.DeleteSessionMemberByExternalId(TenantToken, TenantId, SessionId, ""));
        Assert.Contains("externalMemberId", exception.ParamName);
    }

    [Fact]
    public async Task DeleteSessionMemberByExternalId_InvalidId()
    {
        await ClassUnderTest.DeleteSessionMemberByExternalId(TenantToken, TenantId, SessionId, "123456789a");
    }

    [Fact]
    public async Task DeleteSessionMemberByExternalId()
    {
        var request = Fixture.Create<AddSessionMemberRequest>();
        request.Email = "test1@southportsolutions.com";
        await ClassUnderTest.AddSessionMember(TenantToken, TenantId, SessionId, request);

        var apiResponse = await ClassUnderTest.DeleteSessionMemberByExternalId(TenantToken, TenantId, SessionId, request.ExternalMemberId);
        Assert.True(apiResponse.IsSuccess);
    }

    #endregion

    #region Get Sessions

    [Fact]
    public async Task GetSessions_MissingTenantToken()
    {
        var exception = await Assert.ThrowsAsync<ArgumentNullException>(() => ClassUnderTest.GetSessions("", TenantId, DateTime.MinValue));
        Assert.Equal("tenantToken", exception.ParamName);
    }

    [Fact]
    public async Task GetSessions_MissingTenantId()
    {
        var exception = await Assert.ThrowsAsync<ArgumentNullException>(() => ClassUnderTest.GetSessions(TenantToken, "", DateTime.MinValue));
        Assert.Equal("tenantId", exception.ParamName);
    }

    [Fact]
    public async Task GetSessions_InvalidTenantId()
    {
        var exception = await ClassUnderTest.GetSessions(TenantToken, "123456789a", DateTime.MinValue);
        Assert.Contains("4003", exception.ErrorMessage);
    }

    [Fact]
    public async Task GetSessions()
    {
        var response = await ClassUnderTest.GetSessions(TenantToken, TenantId, DateTime.MinValue);
        Assert.NotNull(response);
        Assert.NotEmpty(response.Data.Data);
        Assert.Null(response.Data.NextUri);
    }

    #endregion

    #region Get Sessions

    [Fact]
    public async Task GetSession_MissingTenantToken()
    {
        var exception = await Assert.ThrowsAsync<ArgumentNullException>(() => ClassUnderTest.GetSession("", TenantId, "lt28Hvo3kg"));
        Assert.Equal("tenantToken", exception.ParamName);
    }

    [Fact]
    public async Task GetSession_MissingTenantId()
    {
        var exception = await Assert.ThrowsAsync<ArgumentNullException>(() => ClassUnderTest.GetSession(TenantToken, "", "lt28Hvo3kg"));
        Assert.Equal("tenantId", exception.ParamName);
    }

    [Fact]
    public async Task GetSession_InvalidTenantId()
    {
        var exception = await ClassUnderTest.GetSession(TenantToken, "123456789a", "lt28Hvo3kg");
        Assert.Contains("4003", exception.ErrorMessage);
    }

    [Fact]
    public async Task GetSession()
    {
        var response = await ClassUnderTest.GetSession(TenantToken, TenantId, "lt28Hvo3kg");
        Assert.NotNull(response);
    }

    #endregion

    #region Get Session Select Items

    [Fact]
    public async Task GetSessionSelectList_MissingTenantToken()
    {
        var exception = await Assert.ThrowsAsync<ArgumentNullException>(() => ClassUnderTest.GetSessionSelectList("", TenantId, DateTime.MinValue));
        Assert.Equal("tenantToken", exception.ParamName);
    }

    [Fact]
    public async Task GetSessionSelectList_MissingTenantId()
    {
        var exception = await Assert.ThrowsAsync<ArgumentNullException>(() => ClassUnderTest.GetSessionSelectList(TenantToken, "", DateTime.MinValue));
        Assert.Equal("tenantId", exception.ParamName);
    }

    [Fact]
    public async Task GetSessionSelectList_InvalidTenantId()
    {
        var exception = await ClassUnderTest.GetSessionSelectList(TenantToken, "123456789a", DateTime.MinValue);
        Assert.Contains("4003", exception.ErrorMessage);
    }

    [Fact]
    public async Task GetSessionSelectList()
    {
        var response = await ClassUnderTest.GetSessionSelectList(TenantToken, TenantId, DateTime.MinValue);
        Assert.NotNull(response);
    }

    #endregion

    #region Get Webhook Subscriptions
    

    [Fact]
    public async Task GetWebhookSubscriptions_MissingTenantToken()
    {
        var exception = await Assert.ThrowsAsync<ArgumentNullException>(() => ClassUnderTest.GetWebhookSubscriptions("", TenantId, CancellationToken.None));
        Assert.Equal("tenantToken", exception.ParamName);
    }

    [Fact]
    public async Task GetWebhookSubscriptions_MissingTenantId()
    {
        var exception = await Assert.ThrowsAsync<ArgumentNullException>(() => ClassUnderTest.GetWebhookSubscriptions(TenantToken, "", CancellationToken.None));
        Assert.Equal("tenantId", exception.ParamName);
    }

    [Fact]
    public async Task GetWebhookSubscriptions_InvalidTenantId()
    {
        var exception = await ClassUnderTest.GetWebhookSubscriptions(TenantToken, "123456789a", CancellationToken.None);
        Assert.Contains("4003", exception.ErrorMessage);
    }

    [Fact]
    public async Task GetWebhookSubscriptions()
    {
        var subscription = new WebhookSubscriptionRequest { EndpointUrl = "https://healthforms.io/webhook", Type = WebhookType.SessionMemberAdded };
        var subscriptionResponse = await ClassUnderTest.AddWebhookSubscription(TenantToken, TenantId, subscription, CancellationToken.None); 

        var response = await ClassUnderTest.GetWebhookSubscriptions(TenantToken, TenantId, CancellationToken.None);
        Assert.NotNull(response.Data);
        Assert.NotEmpty(response.Data);

        var getSubscription = response.Data.Find(c => c.Id == subscriptionResponse.Data.Id);
        Assert.Equal(subscriptionResponse.Data.EndpointUrl, getSubscription.EndpointUrl);
        Assert.Equal(subscriptionResponse.Data.Type, getSubscription.Type);

        await ClassUnderTest.DeleteWebhookSubscription(TenantToken, TenantId, subscriptionResponse.Data.Id, CancellationToken.None);
    }

    #endregion

    #region Add Webhook Subscriptions


    [Fact]
    public async Task AddWebhookSubscriptions_MissingTenantToken()
    {
        var exception = await Assert.ThrowsAsync<ArgumentNullException>(() => ClassUnderTest.AddWebhookSubscription("", TenantId, new WebhookSubscriptionRequest(), CancellationToken.None));
        Assert.Equal("tenantToken", exception.ParamName);
    }

    [Fact]
    public async Task AddWebhookSubscriptions_MissingTenantId()
    {
        var exception = await Assert.ThrowsAsync<ArgumentNullException>(() => ClassUnderTest.AddWebhookSubscription(TenantToken, "", new WebhookSubscriptionRequest(), CancellationToken.None));
        Assert.Equal("tenantId", exception.ParamName);
    }

    [Fact]
    public async Task AddWebhookSubscriptions_InvalidTenantId()
    {
        var exception = await ClassUnderTest.AddWebhookSubscription(TenantToken, "123456789a", new WebhookSubscriptionRequest(), CancellationToken.None);
        Assert.Contains("4003", exception.ErrorMessage);
    }

    [Fact]
    public async Task AddWebhookSubscriptions()
    {
        var subscription = new WebhookSubscriptionRequest { EndpointUrl = "https://healthforms.io/webhook", Type = WebhookType.SessionMemberAdded };
        var subscriptionResponse = await ClassUnderTest.AddWebhookSubscription(TenantToken, TenantId, subscription, CancellationToken.None);

        Assert.NotNull(subscriptionResponse.Data.Id);
        Assert.True(subscriptionResponse.Data.IsActive);
        Assert.Equal(subscriptionResponse.Data.EndpointUrl, subscriptionResponse.Data.EndpointUrl);
        Assert.Equal(subscriptionResponse.Data.Type, subscriptionResponse.Data.Type);

        await ClassUnderTest.DeleteWebhookSubscription(TenantToken, TenantId, subscriptionResponse.Data.Id, CancellationToken.None);
    }

    #endregion

    #region Delete Webhook Subscriptions


    [Fact]
    public async Task DeleteWebhookSubscriptions_MissingTenantToken()
    {
        var exception = await Assert.ThrowsAsync<ArgumentNullException>(() => ClassUnderTest.DeleteWebhookSubscription("", TenantId, "id123", CancellationToken.None));
        Assert.Equal("tenantToken", exception.ParamName);
    }

    [Fact]
    public async Task DeleteWebhookSubscriptions_MissingTenantId()
    {
        var exception = await Assert.ThrowsAsync<ArgumentNullException>(() => ClassUnderTest.DeleteWebhookSubscription(TenantToken, "", "id123", CancellationToken.None));
        Assert.Equal("tenantId", exception.ParamName);
    }

    [Fact]
    public async Task DeleteWebhookSubscriptions_InvalidTenantId()
    {
        var exception = await ClassUnderTest.DeleteWebhookSubscription(TenantToken, "123456789a", "id12345678", CancellationToken.None);
        Assert.Contains("4003", exception.ErrorMessage);
    }

    [Fact]
    public async Task DeleteWebhookSubscriptions()
    {
        var subscription = new WebhookSubscriptionRequest { EndpointUrl = "https://healthforms.io/webhook", Type = WebhookType.SessionMemberAdded };
        var subscriptionResponse = await ClassUnderTest.AddWebhookSubscription(TenantToken, TenantId, subscription, CancellationToken.None);

        await ClassUnderTest.DeleteWebhookSubscription(TenantToken, TenantId, subscriptionResponse.Data.Id, CancellationToken.None);
    }

    #endregion
}