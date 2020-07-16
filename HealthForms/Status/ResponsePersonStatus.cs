using System;

namespace HealthForms.Status
{
    public class ResponsePersonStatus : IResponsePersonStatus
    {
        public Guid Id { get; set; }
        public string ExternalId { get; set; }
        public string FormStatus { get; set; }
        public DateTime? FormUpdatedOn { get; set; }
        public string InvitationStatus { get; set; }
        public DateTime? InvitationUpdatedOn { get; set; }
    }
}