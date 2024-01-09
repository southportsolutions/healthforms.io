using HealthForms.Api.Core.Models.SessionMember.Interfaces;

namespace HealthForms.Api.Core.Models.SessionMember;

public class SessionMemberResponse : ISessionMemberResponse
{
    public string? Id { get; set; }
    public string? ExternalId { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Status { get; set; }
    public IEnumerable<ISessionMemberFormResponse> Forms { get; set; } = new List<SessionMemberFormResponse>();
}