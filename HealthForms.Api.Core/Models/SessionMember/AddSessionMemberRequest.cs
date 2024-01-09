using HealthForms.Api.Core.Models.SessionMember.Interfaces;

namespace HealthForms.Api.Core.Models.SessionMember
{
    public class AddSessionMemberRequest : IAddSessionMemberRequest
    {
        public string? ExternalMemberId { get; set; }
        public string ExternalAttendeeId { get; set; } = string.Empty;
        public string? Group { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public DateTime SendInvitationOn { get; set; }
    }
}