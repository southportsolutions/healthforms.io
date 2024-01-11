namespace HealthForms.Api.Core.Models.SessionMember;

public class AddSessionMemberBulk
{
    public IEnumerable<AddSessionMemberResponse> AddedMembers { get; set; } = new List<AddSessionMemberResponse>();
    public IEnumerable<AddSessionMemberErrorResponse> Errors { get; set; } = new List<AddSessionMemberErrorResponse>();
}