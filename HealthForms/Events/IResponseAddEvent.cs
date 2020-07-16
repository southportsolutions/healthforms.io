using System;

namespace HealthForms.Events
{
    public interface IResponseAddEvent
    {
        Guid Id { get; set; }
    }
}