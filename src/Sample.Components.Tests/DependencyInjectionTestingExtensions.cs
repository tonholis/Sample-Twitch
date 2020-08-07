namespace Sample.Components.Tests
{
    using System;
    using Automatonymous;
    using MassTransit;
    using MassTransit.Definition;
    using MassTransit.ExtensionsDependencyInjectionIntegration;
    using MassTransit.Saga;
    using MassTransit.Testing;
    using Microsoft.Extensions.DependencyInjection;


  public static class DependencyInjectionTestingExtensions
    {
        public static IServiceCollection AddInMemoryTestHarness(this IServiceCollection services,
            Action<IServiceCollectionBusConfigurator> configure = null)
        {
            services.AddSingleton(provider =>
            {
                var testHarness = new InMemoryTestHarness();

                var busRegistrationContext = provider.GetRequiredService<IBusRegistrationContext>();
                testHarness.OnConfigureInMemoryBus += configurator => configurator.ConfigureEndpoints(busRegistrationContext);

                return testHarness;
            });
            services.AddSingleton<BusTestHarness>(provider => provider.GetRequiredService<InMemoryTestHarness>());

            services.AddMassTransit(cfg =>
            {
                configure?.Invoke(cfg);

                cfg.AddBus(context => context.GetRequiredService<InMemoryTestHarness>().BusControl);
            });

            return services;
        }

        public static IServiceCollection AddSagaTestHarness<T>(this IServiceCollection services)
            where T : class, ISaga
        {
            services.AddSingleton<InMemorySagaRepository<T>>();
            services.AddSingleton(provider =>
            {
                var testHarness = provider.GetRequiredService<BusTestHarness>();
                var sagaRepository = provider.GetRequiredService<InMemorySagaRepository<T>>();
            
                var formatter = provider.GetService<IEndpointNameFormatter>() ?? DefaultEndpointNameFormatter.Instance;
            
                string queueName = formatter.Saga<T>();
            
                return new ContainerSagaTestHarness<T>(testHarness, sagaRepository, queueName);
            });
            
            services.AddSingleton<SagaTestHarness<T>>(provider => provider.GetRequiredService<ContainerSagaTestHarness<T>>());
            services.AddSingleton(provider => provider.GetRequiredService<ContainerSagaTestHarness<T>>().Repository);
          
            return services;
        }

        class ContainerSagaTestHarness<TSaga> :
            SagaTestHarness<TSaga>
            where TSaga : class, ISaga
        {
            public ContainerSagaTestHarness(BusTestHarness testHarness, ISagaRepository<TSaga> repository, string queueName)
                : base(testHarness, repository, queueName)
            {
            }

            public ISagaRepository<TSaga> Repository => TestRepository;
        }
        
        
        public static IServiceCollection AddStateMachineSagaTestHarness<TInstance, TStateMachine>(this IServiceCollection services)
            where TInstance : class, SagaStateMachineInstance
            where TStateMachine : SagaStateMachine<TInstance>, new()
        {
            services.AddSingleton<InMemorySagaRepository<TInstance>>();
            
            services.AddSingleton(provider =>
            {
                var testHarness = provider.GetRequiredService<BusTestHarness>();
                var sagaRepository = provider.GetRequiredService<InMemorySagaRepository<TInstance>>();
                var stateMachine = new TStateMachine();
                
                var formatter = provider.GetService<IEndpointNameFormatter>() ?? DefaultEndpointNameFormatter.Instance;
                string queueName = formatter.Saga<TInstance>();
                
                return new ContainerStateMachineSagaTestHarness<TInstance,TStateMachine>(testHarness, sagaRepository, stateMachine, queueName);
            });
            
            services.AddSingleton<StateMachineSagaTestHarness<TInstance, TStateMachine>>(provider => provider.GetRequiredService<ContainerStateMachineSagaTestHarness<TInstance,TStateMachine>>());
            services.AddSingleton(provider => provider.GetRequiredService<ContainerStateMachineSagaTestHarness<TInstance,TStateMachine>>().Repository);
  
            return services;
        }
        
        class ContainerStateMachineSagaTestHarness<TInstance, TStateMachine> :
            StateMachineSagaTestHarness<TInstance, TStateMachine>
            where TInstance : class, SagaStateMachineInstance
            where TStateMachine : SagaStateMachine<TInstance>
        {
            public ContainerStateMachineSagaTestHarness(BusTestHarness testHarness, ISagaRepository<TInstance> repository, TStateMachine stateMachine, string queueName)
                : base(testHarness, repository, stateMachine, queueName)
            {
            }

            public ISagaRepository<TInstance> Repository => TestRepository;
        }
        
    }
}