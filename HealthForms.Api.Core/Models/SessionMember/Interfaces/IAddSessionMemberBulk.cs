using HealthForms.Api.Core.Models.SessionMember;

namespace HealthForms.Api.Core.Models.SessionMember.Interfaces;

public interface IAddSessionMemberBulk
{
    IEnumerable<IAddSessionMemberResponse> AddedMembers { get; set; }
    IEnumerable<IAddSessionMemberErrorResponse> Errors { get; set; }
}