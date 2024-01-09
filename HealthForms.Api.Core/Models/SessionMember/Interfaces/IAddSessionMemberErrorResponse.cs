using HealthForms.Api.Core.Models.SessionMember;

namespace HealthForms.Api.Core.Models.SessionMember.Interfaces;

public interface IAddSessionMemberErrorResponse
{
    string? Error { get; set; }
    AddSessionMemberRequest? Member { get; set; }
}