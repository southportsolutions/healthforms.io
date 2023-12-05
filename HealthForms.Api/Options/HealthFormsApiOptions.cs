using System;
using System.Collections.Generic;
using System.Text;

namespace HealthForms.Api.Options
{
    public class HealthFormsApiOptions
    {
        public const string Key = "HeatlhForms";
        public string HostAddress { get; set; } = "https://app.healtforms.io/";
        public string HostAddressAuth => $"{HostAddress}account/";
        public string HostAddressApi => $"{HostAddress}api/";
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string Scopes { get; set; } = "openid profile offline";
    }
}
