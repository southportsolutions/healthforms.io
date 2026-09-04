namespace HealthForms.Api.Core.Models.Users;

/// <summary>Adds a user, or updates the user that already has this email address. Roles are added, never removed.</summary>
public class AddUserRequest
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string EmailAddress { get; set; } = string.Empty;
    public List<AddUserRoleRequest> Roles { get; set; } = new();
}
