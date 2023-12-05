using HealthForms.Api.Core.Models.Interfaces;

namespace HealthForms.Api.Core.Models;

public class FormResponse : IFormResponse
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string? Instructions { get; set; }
    public int ValidForMonths { get; set; }
    public bool IsEnabled { get; set; }
    public bool IsFormDateRequired { get; set; }
    public string? FormDateLabel { get; set; }
    public string? FormDateHelpText { get; set; }
}