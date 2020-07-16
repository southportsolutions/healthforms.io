using System;

namespace HealthForms.Events
{
    public interface IRequestAddEvent
    {
        string Name { get; set; }
        string Description { get; set; }
        DateTime StartDateTime { get; set; }
        DateTime EndDateTime { get; set; }
        DateTime? FormDeadline { get; set; }
        DateTime? InvitationSendDate { get; set; }
    }
}