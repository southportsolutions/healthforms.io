using HealthForms.Api.Core.Models.SessionMember.Interfaces;

namespace HealthForms.Api.Core.Models.SessionMember;

public class AddSessionMemberResponse : IAddSessionMemberResponse
{
    public string? AttendeeId { get; set; }
    public string? MemberId { get; set; }
    public string? SessionId { get; set; }
    public DateTime? InvitationSendOn { get; set; }
    public DateTime? InvitationSentOn { get; set; }
    public bool Accepted { get; set; } = false;
}