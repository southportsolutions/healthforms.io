namespace HealthForms.Api.Core.Models.Webhooks;

public class WebhookData<T> where T : class
{
    public DateTime Timestamp { get; set; }
    public WebhookType Type { get; set; }
    public string EventId { get; set; }
    public T Data { get; set; }
}