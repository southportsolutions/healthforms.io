namespace HealthForms.Api.Core.Models.Sessions;

public class SessionCreateRequest : SessionCreateRequestBase
{
    public override string Name { get; set; }
    public override DateTime StartDate { get; set; }
    public override DateTime EndDate { get; set; }
    public override string Description { get; set; }
    public override DateTime? InvitationSendDate { get; set; }
    public override List<SessionFormTypeRequest> FormTypeIds { get; set; } = new();
}