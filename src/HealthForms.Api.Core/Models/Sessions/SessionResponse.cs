namespace HealthForms.Api.Core.Models.Sessions;

public class SessionResponse
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string? Description { get; set; } = "";
    public string SupportEmailAddress { get; set; } = "";
    public DateTime? SendInvitationsOn { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public List<SessionFormResponse> Forms { get; set; } = new();
}