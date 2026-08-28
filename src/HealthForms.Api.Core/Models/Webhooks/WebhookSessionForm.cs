using HealthForms.Api.Core.Models.Sessions;

namespace HealthForms.Api.Core.Models.Webhooks;

public class WebhookSessionForm : WebhookData<SessionFormResponse>
{
    public WebhookSessionForm(string eventId, SessionFormResponse data) : base(eventId, data)
    {
    }
}