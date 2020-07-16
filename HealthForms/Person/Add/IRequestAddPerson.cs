using System;

namespace HealthForms.Person.Add
{
    public interface IRequestAddPerson
    {
        string FirstName { get; set; }
        string LastName { get; set; }
        string EmailAddress { get; set; }
        string ExternalId { get; set; }
        DateTime InvitationSendDateTime { get; set; }
    }
}
