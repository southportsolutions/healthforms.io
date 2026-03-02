namespace HealthForms.Api.Core.Models.FormPacket;

public class FormPacketResponse
{
    public string? Id { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public List<string>? FormTypeIds { get; set; }
}
