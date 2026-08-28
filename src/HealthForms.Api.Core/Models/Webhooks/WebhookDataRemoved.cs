namespace HealthForms.Api.Core.Models.Webhooks;

public class WebhookDataRemoved(string eventId, string id)
{
    public DateTime Timestamp { get; set; }
    public WebhookType Type { get; set; }
    public string EventId { get; set; } = eventId;
    public string Id { get; set; } = id;
}