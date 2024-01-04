using AutoFixture;
using AutoFixture.AutoMoq;
using HealthForms.Api.Options;

namespace HealthForms.Api.Tests
{
    public abstract class UnitTestBase<T> where T : class
    {
        protected IFixture Fixture { get; } = new Fixture().Customize(new AutoMoqCustomization());
        protected T ClassUnderTest { get; set; }

        public HealthFormsApiOptions HealthFormsApiOptions { get; }

        protected UnitTestBase()
        {
            HealthFormsApiOptions = Startup.GetOptions();

        }

    }
}