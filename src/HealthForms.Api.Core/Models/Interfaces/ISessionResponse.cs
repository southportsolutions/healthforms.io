namespace HealthForms.Api.Core.Models.Interfaces;

public interface ISessionResponse
{
    string Id { get; set; }
    string Name { get; set; }
    string? Description { get; set; }
    string SupportEmailAddress { get; set; }
    DateTime StartDate { get; set; }
    DateTime EndDate { get; set; }
    List<IFormResponse> Forms { get; set; }
}