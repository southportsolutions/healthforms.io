using HealthForms.Api.Core.Models.SessionMember;

namespace HealthForms.Api.Core.Models.Webhooks;

public class WebhookSessionMemberForm : WebhookData<SessionMemberFormResponse>
{
    public WebhookSessionMemberForm(string eventId, SessionMemberFormResponse data) : base(eventId, data)
    {
    }
}
