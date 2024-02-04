namespace HealthForms.Api.Core.Models.SessionMember;

public class AddSessionMemberRequest
{
    public string? ExternalMemberId { get; set; }
    public string? ExternalAttendeeId { get; set; }
    public string? Group { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public DateTime? SendInvitationOn { get; set; }
}