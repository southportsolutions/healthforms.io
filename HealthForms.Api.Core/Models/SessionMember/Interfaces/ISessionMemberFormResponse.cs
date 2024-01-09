namespace HealthForms.Api.Core.Models.SessionMember.Interfaces;

public interface ISessionMemberFormResponse
{
    string? FormId { get; set; }
    string? FormName { get; set; }
    string? FormStatus { get; set; }
}