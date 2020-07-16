using System;

namespace HealthForms.Person.Get
{
    public class ResponseGetPerson : IResponseGetPerson
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string EmailAddress { get; set; }
        public string ExternalId { get; set; }
        public string OrganizationId { get; set; }
        public string FormStatus { get; set; }
        public DateTime? FormUpdatedOn { get; set; }
        public string InvitationStatus { get; set; }
        public DateTime? InvitationUpdatedOn { get; set; }
    }
}
