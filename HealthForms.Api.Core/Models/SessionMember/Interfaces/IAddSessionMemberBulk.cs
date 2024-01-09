using HealthForms.Api.Core.Models.SessionMember;

namespace HealthForms.Api.Core.Models.SessionMember.Interfaces;

public interface IAddSessionMemberBulk
{
    IEnumerable<AddSessionMemberResponse> AddedMembers { get; set; }
    IEnumerable<AddSessionMemberErrorResponse> Errors { get; set; }
}