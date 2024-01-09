namespace HealthForms.Api.Core.Models.SessionMember.Interfaces;

public interface IAddSessionMemberResponse
{
    string? AttendeeId { get; set; }
    string? MemberId { get; set; }
    string? SessionId { get; set; }
    DateTime? InvitationSendOn { get; set; }
    DateTime? InvitationSentOn { get; set; }
    bool Accepted { get; set; }
}