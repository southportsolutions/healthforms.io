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

    /// <summary>
    /// The link the participant opens to accept the invitation.
    /// Null when the invitation has been accepted or the session has ended.
    /// </summary>
    public string? InvitationUrl { get; set; }

    /// <summary>
    /// The code the participant can enter on the invitation page.
    /// Null when the invitation has been accepted or the session has ended.
    /// </summary>
    public string? InvitationCode { get; set; }

    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Status { get; set; }
    public string? StatusDescription { get; set; }
    public bool IsComplete { get; set; }
    public List<string>? Tags { get; set; }
    public IEnumerable<string> FormPacketIds { get; set; } = new List<string>();
    public IEnumerable<SessionMemberFormResponse> Forms { get; set; } = new List<SessionMemberFormResponse>();
}