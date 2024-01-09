using HealthForms.Api.Options;

namespace HealthForms.Api.Tests.Options
{
    public class HealthFormsApiTestOptions : HealthFormsApiOptions
    {
        //public override string HostAddressAuth => $"{HostAddress}dev/account/";
        public override string HostAddressAuth => $"https://localhost:5001/";
        //public override string HostAddressApi => $"{HostAddress}dev/api/";
        public override string HostAddressApi => $"https://localhost:5007/";
        public string TenantToken { get; set; }
        public string SessionId { get; set; }
        public string TenantId { get; set; }
        public string AttendeeId { get; set; }
    }
}
