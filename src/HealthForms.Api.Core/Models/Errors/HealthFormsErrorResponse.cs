namespace HealthForms.Api.Core.Models.Errors
{
    public class HealthFormsErrorResponse
    {
        public string Message { get; set; } = string.Empty;
        public IEnumerable<HealthFormsValidationError>? ValidationErrors { get; set; }
    }
}