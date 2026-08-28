using HealthForms.Api.Core.Models.Sessions;

namespace HealthForms.Api.Core.Models.Webhooks;

public class WebhookSession : WebhookData<SessionResponse>
{
    public WebhookSession(string eventId, SessionResponse data) : base(eventId, data)
    {
    }
}