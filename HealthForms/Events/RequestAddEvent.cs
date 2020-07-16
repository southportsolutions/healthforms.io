using System;

namespace HealthForms.Events
{
    public class RequestAddEvent : IRequestAddEvent
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime StartDateTime { get; set; }
        public DateTime EndDateTime { get; set; }
        public DateTime? FormDeadline { get; set; }
        public DateTime? InvitationSendDate { get; set; }
        public string ExternalId { get; set; }
    }
}
