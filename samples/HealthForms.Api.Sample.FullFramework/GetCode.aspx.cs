using HealthForms.Api.Clients;
using System;
using System.Net.Http;
using System.Web.UI;
using HealthForms.Api.Sample.FullFramework.Shared;
using System.Web;

namespace HealthForms.Api.Sample.FullFramework
{
    public partial class GetCode : Page
    {
        private readonly HealthFormsApiHttpClientDisposable _healthFormsApiClient;

        public GetCode()
        {
            _healthFormsApiClient = new HealthFormsApiHttpClientDisposable(new HttpClient(), Shared.Options.HealthFormsApiOptions);
        }

        protected void Page_Load(object sender, EventArgs e)
        {

        }

        public override void Dispose()
        {
            base.Dispose();
            _healthFormsApiClient.Dispose();
        }

        protected void SubmitButton_Click(object sender, EventArgs e)
        {
            if (!ModelState.IsValid)
            {
                return;
            }
            var tenantId = txtTenantId.Text;
            var redirectData = _healthFormsApiClient.GetRedirectUrl(tenantId);
            PersistentData.TenantCodeVerifications[tenantId] = redirectData.CodeVerifier;
            HttpContext.Current.Response.Redirect(redirectData.Uri, false);
        }
    }
}