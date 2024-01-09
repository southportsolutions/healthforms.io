using HealthForms.Api.Core.Models.SessionMember.Interfaces;

namespace HealthForms.Api.Core.Models.SessionMember;

public class AddSessionMemberErrorResponse : IAddSessionMemberErrorResponse
{
    public string? Error { get; set; }
    public AddSessionMemberRequest? Member { get; set; }
}