namespace HealthForms.Api.Core.Models.SessionMember;

public class AddSessionMemberErrorResponse
{
    public string? Error { get; set; }
    public AddSessionMemberRequest? Member { get; set; }
}