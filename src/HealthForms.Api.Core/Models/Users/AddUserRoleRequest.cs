namespace HealthForms.Api.Core.Models.Users;

public class AddUserRoleRequest
{
    public UserRole Role { get; set; }
    public string SessionId { get; set; } = string.Empty;
    /// <summary>Omit to grant the role for the whole session.</summary>
    public string? GroupId { get; set; }
}
