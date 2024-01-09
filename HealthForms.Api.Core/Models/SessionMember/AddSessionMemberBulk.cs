using HealthForms.Api.Core.Models.SessionMember.Interfaces;

namespace HealthForms.Api.Core.Models.SessionMember;

public class AddSessionMemberBulk : IAddSessionMemberBulk
{
    public IEnumerable<IAddSessionMemberResponse> AddedMembers { get; set; } = new List<AddSessionMemberResponse>();
    public IEnumerable<IAddSessionMemberErrorResponse> Errors { get; set; } = new List<AddSessionMemberErrorResponse>();
}