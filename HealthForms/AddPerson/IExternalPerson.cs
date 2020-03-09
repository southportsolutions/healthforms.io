namespace HealthForms.AddPerson
{
    public interface IExternalPerson
    {
        string FirstName { get; set; }
        string LastName { get; set; }
        string EmailAddress { get; set; }
        string ExternalId { get; set; }
        string GroupId { get; set; }
    }
}
