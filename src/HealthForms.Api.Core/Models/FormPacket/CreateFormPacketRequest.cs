namespace HealthForms.Api.Core.Models.FormPacket;

public class CreateFormPacketRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public List<string>? FormTypeIds { get; set; }
}
