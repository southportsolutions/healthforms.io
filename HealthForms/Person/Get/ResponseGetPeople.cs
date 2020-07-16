using System.Collections.Generic;

namespace HealthForms.Person.Get
{
    public class ResponseGetPeople : IResponseList<ResponseGetPerson>
    {
        public string Next { get; set; }
        public List<ResponseGetPerson> People { get; set; }
    }
}
