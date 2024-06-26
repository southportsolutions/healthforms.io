namespace HealthForms.Api.Core.Models.SessionMember;

public enum SessionMemberSearchResult
{
    None,
    Match,
    MatchExternalMemberId,
    MatchExternalAttendeeId,
    ExternalIdConflict
}