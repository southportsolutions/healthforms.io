namespace HealthForms.Api.Core.Models.FormPacket;

public class UpdateFormPacketRequest
{
    public string? Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}
