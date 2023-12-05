using HealthForms.Api.Core.Models.Interfaces;

namespace HealthForms.Api.Core.Models;

public class SessionSelectResponse : ISessionSelectResponse
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}