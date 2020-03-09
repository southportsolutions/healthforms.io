namespace HealthForms.AddPerson
{
    public class ExternalPerson : IExternalPerson
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string EmailAddress { get; set; }
        public string ExternalId { get; set; }
        public string GroupId { get; set; }
    }
}
