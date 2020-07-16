using System;
using System.Collections.Generic;
using System.Text;

namespace HealthForms.Events
{
    public class ResponseAddEvent : IResponseAddEvent
    {
        public Guid Id { get; set; }
    }
}
