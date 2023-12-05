using HealthForms.Api.Core.Models.Interfaces;

namespace HealthForms.Api.Core.Models;

public class MemberWebhook : IMemberWebhook
{
    public string Id { get; set; } = "";
    public string SessionId { get; set; } = "";
    public string? ExternalMemberId { get; set; }
    public string ExternalAttendeeId { get; set; } = "";
    public string Status { get; set; } = "";
}