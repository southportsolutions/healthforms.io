using HealthForms.Api.Options;

namespace HealthForms.Api.Tests.Options
{
    public class HealthFormsApiTestOptions : HealthFormsApiOptions
    {
        private string? _hostAddressAuth;
        public override string HostAddressAuth
        {
            get => _hostAddressAuth ?? $"{HostAddress}dev/account/";
            set => _hostAddressAuth = value;
        }

        private string? _hostAddressApi;
        public override string HostAddressApi
        {
            get => _hostAddressApi ?? $"{HostAddress}dev/api/";
            set => _hostAddressApi = value;
        }
        public string TenantToken { get; set; }
        public string SessionId { get; set; }
        public string TenantId { get; set; }
    }
}
