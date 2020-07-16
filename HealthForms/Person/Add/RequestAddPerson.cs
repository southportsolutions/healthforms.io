using System;

namespace HealthForms.Person.Add
{
    public class RequestAddPerson : IRequestAddPerson
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string EmailAddress { get; set; }
        public string ExternalId { get; set; }
        public DateTime InvitationSendDateTime { get; set; }
    }
}
