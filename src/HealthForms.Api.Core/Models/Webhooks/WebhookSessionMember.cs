using HealthForms.Api.Core.Models.SessionMember;

namespace HealthForms.Api.Core.Models.Webhooks;

public class WebhookSessionMember : WebhookData<SessionMemberResponse>
{
    public WebhookSessionMember(string eventId, SessionMemberResponse data) : base(eventId, data)
    {
    }
}