namespace HealthForms.Api.Core.Models.SessionMember.Interfaces;

public interface IAddSessionMemberRequest
{
    string? ExternalMemberId { get; set; }
    string ExternalAttendeeId { get; set; }
    string? Group { get; set; }
    string FirstName { get; set; }
    string LastName { get; set; }
    string Email { get; set; }
    string? Phone { get; set; }
    DateTime SendInvitationOn { get; set; }
}