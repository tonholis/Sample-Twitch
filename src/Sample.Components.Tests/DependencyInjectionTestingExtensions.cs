namespace Sample.Components.Tests
{
    using System;
    using MassTransit;
    using MassTransit.ExtensionsDependencyInjectionIntegration;
    using MassTransit.Testing;
    using Microsoft.Extensions.DependencyInjection;


    public static class DependencyInjectionTestingExtensions
    {
        public static IServiceCollection AddInMemoryTestHarness(this IServiceCollection services,
            Action<IServiceCollectionBusConfigurator> configure = null)
        {
            services.AddSingleton(provider =>
            {
                var testHarness = new InMemoryTestHarness() { TestTimeout = TimeSpan.FromSeconds(5) };

                var busRegistrationContext = provider.GetRequiredService<IBusRegistrationContext>();
                testHarness.OnConfigureInMemoryBus += configurator => configurator.ConfigureEndpoints(busRegistrationContext);

                return testHarness;
            });

            services.AddMassTransit(cfg =>
            {
                configure?.Invoke(cfg);

                cfg.AddBus(context => context.GetRequiredService<InMemoryTestHarness>().BusControl);
            });

            return services;
        }
    }
}