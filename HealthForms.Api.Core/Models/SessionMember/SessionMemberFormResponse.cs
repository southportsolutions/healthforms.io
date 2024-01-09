using HealthForms.Api.Core.Models.SessionMember.Interfaces;

namespace HealthForms.Api.Core.Models.SessionMember;

public class SessionMemberFormResponse : ISessionMemberFormResponse
{
    public string? FormId { get; set; }
    public string? FormName { get; set; }
    public string? FormStatus { get; set; }
}