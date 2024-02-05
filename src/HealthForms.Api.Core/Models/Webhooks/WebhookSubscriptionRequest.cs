namespace HealthForms.Api.Core.Models.Webhooks;

public class WebhookSubscriptionRequest
{
    public WebhookType Type { get; set; }
    public string EndpointUrl { get; set; }
}