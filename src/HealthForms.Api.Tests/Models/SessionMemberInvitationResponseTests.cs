using HealthForms.Api.Core.Models.SessionMember;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace HealthForms.Api.Tests.Models;

public class SessionMemberInvitationResponseTests
{
    // Same naming policy and reference handling as the client's _jsonOptions in HealthFormsApiHttpClient.
    private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, ReferenceHandler = ReferenceHandler.IgnoreCycles };

    [Fact]
    public void Deserialize_CamelCaseJson_SetsAllFiveProperties()
    {
        var json = "{\"invitationUrl\":\"https://auth.test/invitations?c=AB12345678\",\"invitationCode\":\"AB12345678\",\"invitationAccepted\":true,\"invitationSendOn\":\"2026-11-20T09:30:00.0000000Z\",\"invitationSentOn\":\"2026-11-21T10:15:00.0000000Z\"}";

        var invitation = JsonSerializer.Deserialize<SessionMemberInvitationResponse>(json, JsonOptions);

        Assert.NotNull(invitation);
        Assert.Equal("https://auth.test/invitations?c=AB12345678", invitation.InvitationUrl);
        Assert.Equal("AB12345678", invitation.InvitationCode);
        Assert.True(invitation.InvitationAccepted);
        Assert.Equal(new DateTime(2026, 11, 20, 9, 30, 0), invitation.InvitationSendOn);
        Assert.Equal(new DateTime(2026, 11, 21, 10, 15, 0), invitation.InvitationSentOn);
    }

    [Fact]
    public void Deserialize_NullUrlAndCode_LeavesInvitationUrlAndInvitationCodeNull()
    {
        var json = "{\"invitationUrl\":null,\"invitationCode\":null,\"invitationAccepted\":true}";

        var invitation = JsonSerializer.Deserialize<SessionMemberInvitationResponse>(json, JsonOptions);

        Assert.NotNull(invitation);
        Assert.Null(invitation.InvitationUrl);
        Assert.Null(invitation.InvitationCode);
        Assert.True(invitation.InvitationAccepted);
    }
}
