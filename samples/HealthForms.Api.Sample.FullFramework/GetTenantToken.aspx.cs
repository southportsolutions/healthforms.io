using HealthForms.Api.Clients;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.UI;

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

        }

        protected async Task Post

        public override void Dispose()
        {
            base.Dispose();
            _healthFormsApiClient.Dispose();
        }
    }
}