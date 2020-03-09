using System;
using Newtonsoft.Json;

namespace HealthForms
{
    public interface IPersonStatus
    {
        [JsonProperty(propertyName: "personId")]
        int PersonId { get; set; }
        [JsonProperty(propertyName: "eventId")]
        int EventId { get; set; }
        [JsonProperty(propertyName: "externalId")]
        int ExternalId { get; set; }
        [JsonProperty(propertyName: "statusCode")]
        int StatusCode { get; set; }
        [JsonProperty(propertyName: "updatedOn")]
        DateTime UpdatedOn { get; set; }
    }
}
