namespace HealthForms.Api.Core.Models.Sessions;

public class SessionFormTypeRequest : SessionFormTypeRequestBase
{
    public override string? FormTypeId { get; set; }
    public override string? ReviewWorkflowId { get; set; }
    public override int? DisplayOrder { get; set; }
    public override DateTime? SubmissionDeadline { get; set; }
    public override bool IsRequired { get; set; }
    public override string? SubmissionInstructions { get; set; }
}