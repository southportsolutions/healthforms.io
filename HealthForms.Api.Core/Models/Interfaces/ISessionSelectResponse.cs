namespace HealthForms.Api.Core.Models.Interfaces;

public interface ISessionSelectResponse
{
    string Id { get; set; }
    string Name { get; set; }
    DateTime StartDate { get; set; }
    DateTime EndDate { get; set; }
}