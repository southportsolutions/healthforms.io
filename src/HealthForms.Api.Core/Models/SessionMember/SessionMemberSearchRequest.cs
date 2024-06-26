namespace HealthForms.Api.Core.Models.SessionMember;

public class SessionMemberSearchRequest
{
    public string? ExternalMemberId { get; set; }
    public string? ExternalAttendeeId { get; set; }
}