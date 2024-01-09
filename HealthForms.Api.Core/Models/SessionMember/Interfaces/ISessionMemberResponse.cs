namespace HealthForms.Api.Core.Models.SessionMember.Interfaces;

public interface ISessionMemberResponse
{
    string? Id { get; set; }
    string? ExternalId { get; set; }
    string? FirstName { get; set; }
    string? LastName { get; set; }
    string? Status { get; set; }
    IEnumerable<SessionMemberFormResponse> Forms { get; set; }
}