using System.Collections.Generic;

namespace HealthForms
{
    public interface IResponseList<T> where T : class
    {
        string Next { get; set; }
        List<T> People { get; set; }
    }
}