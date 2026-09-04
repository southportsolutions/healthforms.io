namespace HealthForms.Api.Core.Models.Users;

/// <summary>Roles the Public API can grant. Each is scoped to a session and optionally a group.</summary>
public enum UserRole
{
    ParticipantViewer,
    ParticipantFormReviewer,
    ParticipantFormViewer,
}
