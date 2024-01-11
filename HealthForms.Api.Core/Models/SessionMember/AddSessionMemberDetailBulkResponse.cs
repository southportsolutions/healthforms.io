namespace HealthForms.Api.Core.Models.SessionMember;

public class AddSessionMemberDetailBulkResponse
{
    public string MemberId { get; set; }
    public string AttendeeId { get; set; }
    public string ExternalAttendeeId { get; set; }
    public string ExternalMemberId { get; set; }
    public DateTime? InvitationSendOn { get; set; }
    public AddSessionMemberBulkResult? Result { get; set; }
    public string Message { get; set; }
}