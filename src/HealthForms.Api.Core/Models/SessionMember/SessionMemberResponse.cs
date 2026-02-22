namespace HealthForms.Api.Core.Models.SessionMember;

public class SessionMemberResponse
{
    public string? Id { get; set; }
    public string? SessionId { get; set; }
    public string? ExternalAttendeeId { get; set; }
    public string? ExternalMemberId { get; set; }
    public DateTime? InvitationSendOn { get; set; }
    public DateTime? InvitationSentOn { get; set; }
    public bool InvitationAccepted { get; set; } = false;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Status { get; set; }
    public string? StatusDescription { get; set; }
    public bool IsComplete { get; set; }
    public IEnumerable<string> FormPackets { get; set; } = new List<string>();
    public IEnumerable<SessionMemberFormResponse> Forms { get; set; } = new List<SessionMemberFormResponse>();
}