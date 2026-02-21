namespace HealthForms.Api.Core.Models.FormType;

public class CreateFormPacketRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public List<string>? FormTypeIds { get; set; }
}
