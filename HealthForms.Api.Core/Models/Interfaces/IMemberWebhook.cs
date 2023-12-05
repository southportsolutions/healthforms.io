namespace HealthForms.Api.Core.Models.Interfaces;

public interface IMemberWebhook
{
    string Id { get; set; }
    string SessionId { get; set; }
    string? ExternalMemberId { get; set; }
    string ExternalAttendeeId { get; set; }
    string Status { get; set; }
}