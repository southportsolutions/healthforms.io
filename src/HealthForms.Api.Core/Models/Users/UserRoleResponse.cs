namespace HealthForms.Api.Core.Models.Users;

public class UserRoleResponse
{
    public string? Id { get; set; }
    public UserRole Role { get; set; }
    public string? SessionId { get; set; }
    public string? GroupId { get; set; }
}
