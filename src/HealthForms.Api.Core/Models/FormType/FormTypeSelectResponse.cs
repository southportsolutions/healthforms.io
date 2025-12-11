namespace HealthForms.Api.Core.Models.FormType;

public class FormTypeSelectResponse
{
    public string? Id { get; set; }
    public string? Name { get; set; }
    public string? Instructions { get; set; }
    public int? ValidityMonths { get; set; }
}