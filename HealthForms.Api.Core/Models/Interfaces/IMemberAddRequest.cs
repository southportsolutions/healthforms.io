namespace HealthForms.Api.Core.Models.Interfaces;

public interface IMemberAddRequest
{
    string SessionId { get; set; }
    string? ExternalMemberId { get; set; }
    string ExternalAttendeeId { get; set; }
    string? Group { get; set; }
    string FirstName { get; set; }
    string LastName { get; set; }
    string Email { get; set; }
    string? Phone { get; set; }
}