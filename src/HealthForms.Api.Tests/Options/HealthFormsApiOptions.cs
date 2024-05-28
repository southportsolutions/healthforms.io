using HealthForms.Api.Options;

namespace HealthForms.Api.Tests.Options
{
    public class HealthFormsApiTestOptions : HealthFormsApiOptions
    {
        public string TenantToken { get; set; }
        public string SessionId { get; set; }
        public string TenantId { get; set; }
    }
}
