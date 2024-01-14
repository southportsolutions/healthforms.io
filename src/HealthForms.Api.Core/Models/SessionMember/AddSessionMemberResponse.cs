namespace HealthForms.Api.Core.Models.SessionMember;

public class AddSessionMemberResponse
{
    public string? SessionMemberId { get; set; }
    public string? MemberId { get; set; }
    public string? ExternalMemberId { get; set; }
    public string? ExternalAttendeeId { get; set; }
    public string? SessionId { get; set; }
    public DateTime? InvitationSendOn { get; set; }
    public DateTime? InvitationSentOn { get; set; }
    public bool Accepted { get; set; } = false;
}