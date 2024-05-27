namespace HealthForms.Api.Core.Models.SessionMember;

public class SessionMemberFormResponse
{
    public string? Id { get; set; }
    public string? Name { get; set; }
    public string? Status { get; set; }
    public string? StatusDescription { get; set; }
    public bool IsComplete { get; set; }
    public bool IsRequired { get; set; }
}