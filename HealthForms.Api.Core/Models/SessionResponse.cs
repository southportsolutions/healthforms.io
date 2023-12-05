using HealthForms.Api.Core.Models.Interfaces;

namespace HealthForms.Api.Core.Models;

public class SessionResponse : ISessionResponse
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string? Description { get; set; } = "";
    public string SupportEmailAddress { get; set; } = "";
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public List<FormResponse> Forms { get; set; } = new();
}