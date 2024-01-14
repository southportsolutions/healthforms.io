namespace HealthForms.Api.Core.Models;

public class MemberWebhook
{
    public string Id { get; set; } = "";
    public string SessionId { get; set; } = "";
    public string? ExternalMemberId { get; set; }
    public string ExternalAttendeeId { get; set; } = "";
    public string Status { get; set; } = "";
}