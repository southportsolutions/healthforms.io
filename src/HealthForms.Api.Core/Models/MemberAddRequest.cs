using HealthForms.Api.Core.Models.Interfaces;

namespace HealthForms.Api.Core.Models
{
    public class MemberAddRequest : IMemberAddRequest
    {
        public string SessionId { get; set; } = "";
        public string? ExternalMemberId { get; set; }
        public string ExternalAttendeeId { get; set; } = "";
        public string? Group { get; set; }
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
        public string Email { get; set; } = "";
        public string? Phone { get; set; }
    }
}