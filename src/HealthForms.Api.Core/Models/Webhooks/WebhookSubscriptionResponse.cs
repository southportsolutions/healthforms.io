namespace HealthForms.Api.Core.Models.Webhooks;

public class WebhookSubscriptionResponse
{
    public string Id { get; set; }
    public WebhookType Type { get; set; }
    public string EndpointUrl { get; set; }
    public bool IsActive { get; set; }
}