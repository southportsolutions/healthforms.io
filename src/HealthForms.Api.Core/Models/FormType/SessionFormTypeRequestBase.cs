namespace HealthForms.Api.Core.Models.FormType;

public abstract class SessionFormTypeRequestBase
{
    public abstract string? FormTypeId { get; set;  }
    public abstract string? ReviewWorkflowId { get; set; }
    public abstract int? DisplayOrder { get; set; }
    public abstract DateTime? SubmissionDeadline { get; set; }
    public abstract bool IsRequired { get; set; }
    public abstract string? SubmissionInstructions { get; set; }
}