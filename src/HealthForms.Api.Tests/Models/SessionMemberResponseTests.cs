using HealthForms.Api.Core.Models.SessionMember;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace HealthForms.Api.Tests.Models;

public class SessionMemberResponseTests
{
    // Same naming policy and reference handling as the client's _jsonOptions in HealthFormsApiHttpClient.
    private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, ReferenceHandler = ReferenceHandler.IgnoreCycles };

    [Fact]
    public void Deserialize_CamelCaseInvitationFields_SetsInvitationUrlAndInvitationCode()
    {
        var json = "{\"invitationUrl\":\"https://auth.test/invitations?c=AB12345678\",\"invitationCode\":\"AB12345678\"}";

        var sessionMember = JsonSerializer.Deserialize<SessionMemberResponse>(json, JsonOptions);

        Assert.NotNull(sessionMember);
        Assert.Equal("https://auth.test/invitations?c=AB12345678", sessionMember.InvitationUrl);
        Assert.Equal("AB12345678", sessionMember.InvitationCode);
    }

    [Fact]
    public void Deserialize_InvitationFieldsMissing_LeavesInvitationUrlAndInvitationCodeNull()
    {
        var json = "{\"id\":\"x\"}";

        var sessionMember = JsonSerializer.Deserialize<SessionMemberResponse>(json, JsonOptions);

        Assert.NotNull(sessionMember);
        Assert.Null(sessionMember.InvitationUrl);
        Assert.Null(sessionMember.InvitationCode);
    }

    [Fact]
    public void Serialize_InvitationFieldsSet_WritesCamelCaseNames()
    {
        var sessionMember = new SessionMemberResponse
        {
            InvitationUrl = "https://auth.test/invitations?c=AB12345678",
            InvitationCode = "AB12345678"
        };

        var json = JsonSerializer.Serialize(sessionMember, JsonOptions);

        Assert.Contains("\"invitationUrl\"", json);
        Assert.Contains("\"invitationCode\"", json);
    }
}
