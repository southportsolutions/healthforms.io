using System;
using Newtonsoft.Json;

namespace HealthForms.Status
{
    public interface IResponsePersonStatus
    {
        Guid Id { get; set; }
        string ExternalId { get; set; }
        string FormStatus { get; set; }
        DateTime? FormUpdatedOn { get; set; }
        string InvitationStatus { get; set; }
        DateTime? InvitationUpdatedOn { get; set; }
    }
}
