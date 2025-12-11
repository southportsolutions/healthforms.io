
using HealthForms.Api.Core.Models.FormType;

namespace HealthForms.Api.Core.Models.Sessions;

public abstract class SessionCreateRequestBase<TFormTypeRequest> where TFormTypeRequest : SessionFormTypeRequestBase, new()
{
    public abstract string? Name { get; set; }
    public abstract DateTime StartDate { get; set; }
    public abstract DateTime EndDate { get; set; }
    public abstract string? Description { get; set; }
    public abstract DateTime? InvitationSendDate { get; set; }
    public abstract List<TFormTypeRequest> FormTypes { get; set; }
}