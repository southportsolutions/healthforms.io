using HealthForms.Api.Options;

namespace HealthForms.Api.Tests
{
    public abstract class UnitTestBase
    {
        public HealthFormsApiOptions HealthFormsApiOptions { get; }

        protected UnitTestBase()
        {
            HealthFormsApiOptions = Startup.GetOptions();

        }

    }
}