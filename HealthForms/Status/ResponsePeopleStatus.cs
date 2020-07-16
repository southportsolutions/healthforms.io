using System.Collections.Generic;

namespace HealthForms.Status
{
    public class ResponsePeopleStatus : IResponseList<ResponsePeopleStatus>
    {
        public string Next { get; set; }
        public List<ResponsePeopleStatus> People { get; set; }
    }
}
