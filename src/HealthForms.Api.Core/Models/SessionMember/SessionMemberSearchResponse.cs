using System.Text.Json.Serialization;

namespace HealthForms.Api.Core.Models.SessionMember;

public class SessionMemberSearchResponse
{
    [JsonConverter(typeof(JsonStringEnumConverter<SessionMemberSearchResult>))]
    public SessionMemberSearchResult Result { get; set; }
    public SessionMemberResponse? Member { get; set; }
    public SessionMemberResponse? Attendee { get; set; }
}