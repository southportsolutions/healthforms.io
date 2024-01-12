using System.Configuration;
using HealthForms.Api.Options;

namespace HealthForms.Api.Sample.FullFramework.Shared
{
    public static class Options
    {
        private static HealthFormsApiOptions _healthFormsApiOptions;
        public static HealthFormsApiOptions HealthFormsApiOptions =>
            _healthFormsApiOptions ?? (_healthFormsApiOptions = new HealthFormsApiOptions
            {
                ClientId = ConfigurationManager.AppSettings.Get("ClientId"),
                ClientSecret = ConfigurationManager.AppSettings.Get("ClientSecret"),
                HostAddress = ConfigurationManager.AppSettings.Get("HostAddress"),
                RedirectUrl = ConfigurationManager.AppSettings.Get("RedirectUrl"),
            });
    }
}