using System;

namespace HealthForms.Status
{
    public class ExternalExternalPersonStatus : IExternalPersonStatus
    {
        public int PersonId { get; set; }
        public int EventId { get; set; }
        public int ExternalId { get; set; }
        public int StatusCode { get; set; }
        public DateTime UpdatedOn { get; set; }
    }
}