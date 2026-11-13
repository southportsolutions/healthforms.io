namespace HealthForms.Api.Core.Models.SessionMember;

/// <summary>
/// The invitation of one session member, returned by the session member invitation routes.
/// </summary>
public class SessionMemberInvitationResponse
{
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

    /// <summary>
    /// True when the participant has accepted the invitation.
    /// </summary>
    public bool InvitationAccepted { get; set; }

    /// <summary>
    /// The date the invitation email is scheduled to be sent.
    /// </summary>
    public DateTime? InvitationSendOn { get; set; }

    /// <summary>
    /// The date the invitation email was sent. Null when the email has not been sent.
    /// </summary>
    public DateTime? InvitationSentOn { get; set; }
}