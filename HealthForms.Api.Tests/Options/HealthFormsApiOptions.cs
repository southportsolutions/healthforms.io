using HealthForms.Api.Options;

namespace HealthForms.Api.Tests.Options
{
    public class HealthFormsApiTestOptions : HealthFormsApiOptions
    {
        public override string HostAddressAuth => $"{HostAddress}dev/account/";
        public override string HostAddressApi => $"{HostAddress}dev/api/";
    }
}
