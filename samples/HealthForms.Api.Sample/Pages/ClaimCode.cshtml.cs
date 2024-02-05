using HealthForms.Api.Clients;
using HealthForms.Api.Sample.shared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HealthForms.Api.Sample.Pages;

public class ClaimCodeModel(IHealthFormsApiHttpClient healthFormsHttpClient, ILogger<ClaimCodeModel> logger)
    : PageModel
{
    [ModelBinder]
    public string? TenantId { get; set; }
    [ModelBinder]
    public string? Token { get; set; }

    public async Task<IActionResult> OnGet([FromQuery] string code, [FromQuery] string scope)
    {
        if (string.IsNullOrEmpty(code))
        {
            logger.LogError("No code was provided");
            return RedirectToPage(nameof(IndexModel));
        }

        TenantId = healthFormsHttpClient.GetTenantIdFromScope(scope);
        Token = await healthFormsHttpClient.GetTenantToken(code, PersistentData.TenantCodeVerifications[TenantId]);
        PersistentData.TenantCodeVerifications.Remove(TenantId);

        PersistentData.TenantTokens[TenantId] = Token;

        return Page();
    }
}