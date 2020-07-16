using System.Collections.Generic;

namespace HealthForms.Events
{
    public class ResponseGetEvents : IResponseList<ResponseGetEvent>
    {
        public string Next { get; set; }
        public List<ResponseGetEvent> People { get; set; }
    }
}
