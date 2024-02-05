using System.Collections.Generic;

namespace HealthForms.Api.Sample.FullFramework.Shared
{

    public static class PersistentData
    {
        public static readonly Dictionary<string, string> TenantCodeVerifications = new Dictionary<string, string>();
        public static readonly Dictionary<string, string> TenantTokens = new Dictionary<string, string>();
    }
}