namespace HealthForms.Api.Core.Models.Users;

public class UserResponse
{
    public string? Id { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? EmailAddress { get; set; }
    public DateTime InvitedOn { get; set; }
    public DateTime? AcceptedOn { get; set; }
    public bool IsRevoked { get; set; }
    public List<UserRoleResponse> Roles { get; set; } = new();
}
