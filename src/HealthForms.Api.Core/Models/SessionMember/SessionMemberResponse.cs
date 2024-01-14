namespace HealthForms.Api.Core.Models.SessionMember;

public class SessionMemberResponse
{
    public string? Id { get; set; }
    public string ? ExternalAttendeeId { get; set; }
    public string? ExternalMemberId { get; set; }
    public string? ExternalId { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Status { get; set; }
    public bool IsComplete { get; set; }
    public IEnumerable<SessionMemberFormResponse> Forms { get; set; } = new List<SessionMemberFormResponse>();
}