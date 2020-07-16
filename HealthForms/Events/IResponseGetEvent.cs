using System;

namespace HealthForms.Events
{
    public interface IResponseGetEvent
    {
        Guid Id { get; set; }
        string Name { get; set; }
        string Description { get; set; }
        DateTime StartDateTime { get; set; }
        DateTime EndDateTime { get; set; }
        DateTime? FormDeadline { get; set; }
        DateTime? InvitationSendDate { get; set; }
        int CountAttendees { get; set; }
        int CountAttendeeInvitationsAccepted { get; set; }
        int CountAttendeeFormsSubmitted { get; set; }
        int CountAttendeeFormsApproved { get; set; }
    }
}