using HealthForms.Api.Clients;
using HealthForms.Api.Sample.shared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HealthForms.Api.Sample.Pages;

public class ClaimCodeModel : PageModel
{
    private readonly IHealthFormsApiHttpClient _healthFormsHttpClient;
    private readonly ILogger<ClaimCodeModel> _logger;

    public ClaimCodeModel(IHealthFormsApiHttpClient healthFormsHttpClient, ILogger<ClaimCodeModel> logger)
    {
        _healthFormsHttpClient = healthFormsHttpClient;
        _logger = logger;
    }

    [ModelBinder]
    public string? TenantId { get; set; }
    [ModelBinder]
    public string? Token { get; set; }

    public async Task<IActionResult> OnGet([FromQuery] string code, [FromQuery] string scope)
    {
        if (string.IsNullOrEmpty(code))
        {
            _logger.LogError("No code was provided");
            return RedirectToPage(nameof(IndexModel));
        }

        TenantId = _healthFormsHttpClient.GetTenantIdFromScope(scope);
        Token = await _healthFormsHttpClient.GetTenantToken(code, PersistentData.TenantCodeVerifications[TenantId]);
        PersistentData.TenantCodeVerifications.Remove(TenantId);

        PersistentData.TenantTokens[TenantId] = Token;

        return Page();
    }
}