
namespace HealthForms.Api.Core.Models.Sessions;

public abstract class SessionCreateRequestBase
{
    public abstract string? Name { get; set; }
    public abstract DateTime StartDate { get; set; }
    public abstract DateTime EndDate { get; set; }
    public abstract string? Description { get; set; }
    public abstract DateTime? InvitationSendDate { get; set; }
    public abstract List<SessionFormTypeRequest> FormTypes { get; set; }
}