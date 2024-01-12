namespace HealthForms.Api.Core.Models.Interfaces;

public interface IFormResponse
{
    string Id { get; set; }
    string Name { get; set; }
    string? Instructions { get; set; }
    int ValidForMonths { get; set; }
    bool IsEnabled { get; set; }
    bool IsFormDateRequired { get; set; }
    string? FormDateLabel { get; set; }
    string? FormDateHelpText { get; set; }
}