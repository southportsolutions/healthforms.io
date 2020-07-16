using System;
using HealthForms.Status;

namespace HealthForms.Person.Get
{
    public interface IResponseGetPerson : IResponsePersonStatus
    {
        string FirstName { get; set; }
        string LastName { get; set; }
        string EmailAddress { get; set; }
    }
}
