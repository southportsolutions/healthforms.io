namespace HealthForms.Api.Core.Models.FormType;

public class FormPacketResponse
{
    public string? Id { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public List<string>? FormTypeIds { get; set; }
}
