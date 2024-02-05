using HealthForms.Api.Clients;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.UI;
using HealthForms.Api.Sample.FullFramework.Shared;
using System.Web;

namespace HealthForms.Api.Sample.FullFramework
{
    public partial class GetTenantToken : Page
    {
        private readonly HealthFormsApiHttpClientDisposable _healthFormsApiClient;

        public GetTenantToken()
        {
            _healthFormsApiClient = new HealthFormsApiHttpClientDisposable(new HttpClient(), Shared.Options.HealthFormsApiOptions);
        }
        
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                Page.RegisterAsyncTask(new PageAsyncTask(LoadDataAsync));
            }
        }

        private async Task LoadDataAsync()
        {
            var code = Request.QueryString["code"];
            if (string.IsNullOrEmpty(code))
            {
                HttpContext.Current.Response.Redirect("GetCode.aspx", false);
                return;
            }

            var scope = Request.QueryString["scope"];
            TenantId.InnerText = _healthFormsApiClient.GetTenantIdFromScope(scope);
            TenantToken.InnerText = await _healthFormsApiClient.GetTenantToken(code, PersistentData.TenantCodeVerifications[TenantId.InnerText]);
            PersistentData.TenantCodeVerifications.Remove(TenantId.InnerText);

            PersistentData.TenantTokens[TenantId.InnerText] = TenantToken.InnerText;
        }

        public override void Dispose()
        {
            base.Dispose();
            _healthFormsApiClient.Dispose();
        }
    }
}