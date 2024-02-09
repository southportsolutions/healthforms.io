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
        var exception = await Assert.ThrowsAsync<HealthFormsException>(() => ClassUnderTest.GetSessionMembers(TenantToken, "123456789a", SessionId));
        Assert.Contains("4003", exception.Message);
    }

    [Fact]
    public async Task GetSessionMembers()
    {
        var response = await ClassUnderTest.GetSessionMembers(TenantToken, TenantId, SessionId);
        Assert.NotNull(response);
        Assert.NotEmpty(response.Data);
        Assert.NotNull(response.NextUri);


        response = await ClassUnderTest.GetSessionMembers(TenantToken, response.NextUri);
        Assert.NotNull(response);
        Assert.NotEmpty(response.Data);


    }

    [Fact]
    public async Task GetSessionMembers_ExtraPage()
    {
        var response = await ClassUnderTest.GetSessionMembers(TenantToken, TenantId, SessionId, 100);
        Assert.NotNull(response);
        Assert.Empty(response.Data);
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
        Assert.NotNull(response.SessionMemberId);
        Assert.NotNull(response.MemberId);
        Assert.Equal(SessionId, response.SessionId);
        Assert.False(response.Accepted);
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
        var exception = await Assert.ThrowsAsync<HealthFormsException>(() => ClassUnderTest.AddSessionMembers(TenantToken, TenantId, SessionId, request));
        Assert.Contains("3000", exception.Message);
        Assert.Contains(exception.Error.ValidationErrors, c => c.Field == "FirstName");
    }

    [Fact]
    public async Task AddSessionMembers_MissingLastName()
    {
        var request = Fixture.Create<List<AddSessionMemberRequest>>();
        request[0].LastName = "";
        var exception = await Assert.ThrowsAsync<HealthFormsException>(() => ClassUnderTest.AddSessionMembers(TenantToken, TenantId, SessionId, request));
        Assert.Contains("3000", exception.Message);
        Assert.Contains(exception.Error.ValidationErrors, c => c.Field == "LastName");
    }

    [Fact]
    public async Task AddSessionMembers_MissingEmail()
    {
        var request = Fixture.Create<List<AddSessionMemberRequest>>();
        request[0].Email = "";
        var exception = await Assert.ThrowsAsync<HealthFormsException>(() => ClassUnderTest.AddSessionMembers(TenantToken, TenantId, SessionId, request));
        Assert.Contains("3000", exception.Message);
        Assert.Contains(exception.Error.ValidationErrors, c => c.Field == "Email");
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
        Assert.NotNull(response.Id);

        AddSessionMemberBulkResponse status;
        do
        {
            status = await ClassUnderTest.GetAddSessionMembersStatus(TenantToken, TenantId, SessionId, response.Id);
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
        await Assert.ThrowsAsync<HealthFormsException>(() => ClassUnderTest.DeleteSessionMember(TenantToken, TenantId, SessionId, "123456789a"));
    }

    [Fact]
    public async Task DeleteSessionMember()
    {
        var request = Fixture.Create<AddSessionMemberRequest>();
        request.Email = "test1@southportsolutions.com";
        var response = await ClassUnderTest.AddSessionMember(TenantToken, TenantId, SessionId, request);

        var isSuccessful = await ClassUnderTest.DeleteSessionMember(TenantToken, TenantId, SessionId, response.SessionMemberId);
        Assert.True(isSuccessful);
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
        await Assert.ThrowsAsync<HealthFormsException>(() => ClassUnderTest.DeleteSessionMemberByExternalAttendeeId(TenantToken, TenantId, SessionId, "123456789a"));
    }

    [Fact]
    public async Task DeleteSessionMemberByExternalAttendeeId()
    {
        var request = Fixture.Create<AddSessionMemberRequest>();
        request.Email = "test1@southportsolutions.com";
        await ClassUnderTest.AddSessionMember(TenantToken, TenantId, SessionId, request);

        var isSuccessful = await ClassUnderTest.DeleteSessionMemberByExternalAttendeeId(TenantToken, TenantId, SessionId, request.ExternalAttendeeId);
        Assert.True(isSuccessful);
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
        await Assert.ThrowsAsync<HealthFormsException>(() => ClassUnderTest.DeleteSessionMemberByExternalId(TenantToken, TenantId, SessionId, "123456789a"));
    }

    [Fact]
    public async Task DeleteSessionMemberByExternalId()
    {
        var request = Fixture.Create<AddSessionMemberRequest>();
        request.Email = "test1@southportsolutions.com";
        await ClassUnderTest.AddSessionMember(TenantToken, TenantId, SessionId, request);

        var isSuccessful = await ClassUnderTest.DeleteSessionMemberByExternalId(TenantToken, TenantId, SessionId, request.ExternalMemberId);
        Assert.True(isSuccessful);
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
        var exception = await Assert.ThrowsAsync<HealthFormsException>(() => ClassUnderTest.GetSessions(TenantToken, "123456789a", DateTime.MinValue));
        Assert.Contains("4003", exception.Message);
    }

    [Fact]
    public async Task GetSessions()
    {
        var response = await ClassUnderTest.GetSessions(TenantToken, TenantId, DateTime.MinValue);
        Assert.NotNull(response);
        Assert.NotEmpty(response.Data);
        Assert.Null(response.NextUri);
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
        var exception = await Assert.ThrowsAsync<HealthFormsException>(() => ClassUnderTest.GetSession(TenantToken, "123456789a", "lt28Hvo3kg"));
        Assert.Contains("4003", exception.Message);
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
        var exception = await Assert.ThrowsAsync<HealthFormsException>(() => ClassUnderTest.GetSessionSelectList(TenantToken, "123456789a", DateTime.MinValue));
        Assert.Contains("4003", exception.Message);
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
        var exception = await Assert.ThrowsAsync<HealthFormsException>(() => ClassUnderTest.GetWebhookSubscriptions(TenantToken, "123456789a", CancellationToken.None));
        Assert.Contains("4003", exception.Message);
    }

    [Fact]
    public async Task GetWebhookSubscriptions()
    {
        var subscription = new WebhookSubscriptionRequest { EndpointUrl = "https://healthforms.io/webhook", Type = WebhookType.SessionMemberAdded };
        var subscriptionResponse = await ClassUnderTest.AddWebhookSubscription(TenantToken, TenantId, subscription, CancellationToken.None); 

        var response = await ClassUnderTest.GetWebhookSubscriptions(TenantToken, TenantId, CancellationToken.None);
        Assert.NotNull(response);
        Assert.NotEmpty(response);

        var getSubscription = response.Find(c => c.Id == subscriptionResponse.Id);
        Assert.Equal(subscriptionResponse.EndpointUrl, getSubscription.EndpointUrl);
        Assert.Equal(subscriptionResponse.Type, getSubscription.Type);

        await ClassUnderTest.DeleteWebhookSubscription(TenantToken, TenantId, subscriptionResponse.Id, CancellationToken.None);
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
        var exception = await Assert.ThrowsAsync<HealthFormsException>(() => ClassUnderTest.AddWebhookSubscription(TenantToken, "123456789a", new WebhookSubscriptionRequest(), CancellationToken.None));
        Assert.Contains("4003", exception.Message);
    }

    [Fact]
    public async Task AddWebhookSubscriptions()
    {
        var subscription = new WebhookSubscriptionRequest { EndpointUrl = "https://healthforms.io/webhook", Type = WebhookType.SessionMemberAdded };
        var subscriptionResponse = await ClassUnderTest.AddWebhookSubscription(TenantToken, TenantId, subscription, CancellationToken.None);

        Assert.NotNull(subscriptionResponse.Id);
        Assert.True(subscriptionResponse.IsActive);
        Assert.Equal(subscriptionResponse.EndpointUrl, subscriptionResponse.EndpointUrl);
        Assert.Equal(subscriptionResponse.Type, subscriptionResponse.Type);

        await ClassUnderTest.DeleteWebhookSubscription(TenantToken, TenantId, subscriptionResponse.Id, CancellationToken.None);
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
        var exception = await Assert.ThrowsAsync<HealthFormsException>(() => ClassUnderTest.DeleteWebhookSubscription(TenantToken, "123456789a", "id12345678", CancellationToken.None));
        Assert.Contains("4003", exception.Message);
    }

    [Fact]
    public async Task DeleteWebhookSubscriptions()
    {
        var subscription = new WebhookSubscriptionRequest { EndpointUrl = "https://healthforms.io/webhook", Type = WebhookType.SessionMemberAdded };
        var subscriptionResponse = await ClassUnderTest.AddWebhookSubscription(TenantToken, TenantId, subscription, CancellationToken.None);

        await ClassUnderTest.DeleteWebhookSubscription(TenantToken, TenantId, subscriptionResponse.Id, CancellationToken.None);
    }

    #endregion
}