using HealthForms.Api.Clients;
using HealthForms.Api.Sample.shared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HealthForms.Api.Sample.Pages
{
    public class IndexModel(IHealthFormsApiHttpClient healthFormsHttpClient, ILogger<IndexModel> logger)
        : PageModel
    {
        [BindProperty]
        public string TenantId { get; set; } = "ABC";

        public void OnGet()
        {
        }

        public IActionResult OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var redirectData = healthFormsHttpClient.GetRedirectUrl(TenantId);
            PersistentData.TenantCodeVerifications[TenantId] = redirectData.CodeVerifier;
            return Redirect(redirectData.Uri);
        }
    }
}
