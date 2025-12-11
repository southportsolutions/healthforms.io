using HealthForms.Api.Core.Models.FormType;

namespace HealthForms.Api.Core.Models.Sessions;

public class SessionCreateRequest : SessionCreateRequestBase<SessionFormTypeRequest>
{
    public override string? Name { get; set; }
    public override DateTime StartDate { get; set; }
    public override DateTime EndDate { get; set; }
    public override string? Description { get; set; }
    public override DateTime? InvitationSendDate { get; set; }
    public override List<SessionFormTypeRequest> FormTypes { get; set; } = new();
}