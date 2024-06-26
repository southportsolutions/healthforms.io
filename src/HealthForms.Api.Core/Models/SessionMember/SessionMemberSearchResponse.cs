namespace HealthForms.Api.Core.Models.SessionMember;

public class SessionMemberSearchResponse
{
    public SessionMemberSearchResult Result { get; set; }
    public SessionMemberResponse? Member { get; set; }
    public SessionMemberResponse? Attendee { get; set; }
}