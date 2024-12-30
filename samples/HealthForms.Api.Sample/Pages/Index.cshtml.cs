using HealthForms.Api.Clients;
using HealthForms.Api.Sample.shared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HealthForms.Api.Sample.Pages
{
    public class IndexModel : PageModel
    {
        private readonly IHealthFormsApiHttpClient _healthFormsHttpClient;

        public IndexModel(IHealthFormsApiHttpClient healthFormsHttpClient, ILogger<IndexModel> logger)
        {
            _healthFormsHttpClient = healthFormsHttpClient;
        }

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

            var redirectData = _healthFormsHttpClient.GetRedirectUrl(TenantId);
            PersistentData.TenantCodeVerifications[TenantId] = redirectData.CodeVerifier;
            return Redirect(redirectData.Uri);
        }
    }
}
