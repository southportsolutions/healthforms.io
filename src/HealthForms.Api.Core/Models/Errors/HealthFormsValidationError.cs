namespace HealthForms.Api.Core.Models.Errors;

public class HealthFormsValidationError
{
    public string Field { get; set; }
    public string Message { get; set; }
}