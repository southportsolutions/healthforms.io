namespace HealthForms.Api.Errors
{
    public class HealthFormsException : Exception
    {
        public HealthFormsException(string message) : base(message)
        {
        }
    }
}
