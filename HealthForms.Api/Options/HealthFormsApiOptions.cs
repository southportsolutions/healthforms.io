
namespace HealthForms.Api.Options
{
    public class HealthFormsApiOptions
    {
        public const string Key = "HeatlhForms";
        public string HostAddress { get; set; } = "https://app.healthforms.io/";
        public virtual string HostAddressAuth => $"{HostAddress}account/";
        public virtual string HostAddressApi => $"{HostAddress}api/";
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string Scopes { get; set; } = "profile openid offline_access hf_public_all";
        public string RedirectUrl { get; set; }
    }
}
