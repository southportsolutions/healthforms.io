
namespace HealthForms.Api.Options
{
    public class HealthFormsApiOptions
    {
        public const string Key = "HeatlhForms";
        public string HostAddress { get; set; } = "https://app.healthforms.io/";

        private string? _hostAddressAuth;
        public virtual string HostAddressAuth
        {
            get => _hostAddressAuth ?? $"{HostAddress}account/";
            set => _hostAddressAuth = value;
        }

        private string? _hostAddressApi;
        public virtual string HostAddressApi {
            get => _hostAddressApi ?? $"{HostAddress}api/";
            set => _hostAddressApi = value;
        }
        
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string Scopes { get; set; } = "profile openid offline_access hf_public_all customer:{{tenantId}}";
        public string RedirectUrl { get; set; }
    }
}
