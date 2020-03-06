using System;

namespace HealthForms
{
    public class PersonStatus : IPersonStatus
    {
        public int PersonId { get; set; }
        public int EventId { get; set; }
        public int ExternalId { get; set; }
        public int StatusCode { get; set; }
        public DateTime UpdatedOn { get; set; }
    }
}