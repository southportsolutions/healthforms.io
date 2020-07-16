using System;
using System.Collections.Generic;
using System.Text;

namespace HealthForms.Events
{
    public class ResponseGetEvent : IResponseGetEvent
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime StartDateTime { get; set; }
        public DateTime EndDateTime { get; set; }
        public DateTime? FormDeadline { get; set; }
        public DateTime? InvitationSendDate { get; set; }
        public int CountAttendees { get; set; }
        public int CountAttendeeInvitationsAccepted { get; set; }
        public int CountAttendeeFormsSubmitted { get; set; }
        public int CountAttendeeFormsApproved { get; set; }

    }
}
