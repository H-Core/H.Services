using System;
using System.Linq;
using H.Core;
using H.Services.Apps.ViewModels;
using H.Services.Core;
using Microsoft.Extensions.DependencyInjection;
using ReactiveUI;

namespace H.Services.Apps.Initialization
{
    public static class ConfigureServicesExtensions
    {
        public static IServiceCollection AddModules(this IServiceCollection services)
        {
            services = services ?? throw new ArgumentNullException(nameof(services));

            services
                .AddSingleton<IntegrationRunner>();

            return services;
        }

        public static IServiceCollection AddServices(this IServiceCollection services)
        {
            services = services ?? throw new ArgumentNullException(nameof(services));

            services
                .AddSingleton(static provider =>
                    new StaticModuleService(
                        provider.GetServices<IModule>().ToArray()))
                .AddServiceInterface<IServiceBase, StaticModuleService>()
                .AddServiceInterface<IModuleService, StaticModuleService>()
                .AddServiceInterface<ICommandProducer, StaticModuleService>()

                .AddSingleton(static provider =>
                    new RecognitionService(
                        provider.GetServices<IModuleService>().ToArray()))
                .AddServiceInterface<IServiceBase, RecognitionService>()
                .AddServiceInterface<ICommandProducer, RecognitionService>()

                .AddSingleton(static provider => 
                    new RunnerService(
                        provider.GetServices<IModuleService>().ToArray(), 
                        provider.GetServices<ICommandProducer>().ToArray()))
                .AddServiceInterface<IServiceBase, RunnerService>()

                .AddSingleton<HookService>()
                .AddServiceInterface<IServiceBase, HookService>()
                .AddServiceInterface<ICommandProducer, HookService>()
                .AddServiceInterface<IProcessCommandProducer, HookService>()

                .AddSingleton(static _ => new DeskbandService
                {
                    ConnectedCommandFactory = _ => new Command("print", "Connected to H.DeskBand."),
                    DisconnectedCommandFactory = _ => new Command("print", "Disconnected from H.DeskBand."),
                })
                .AddServiceInterface<IServiceBase, DeskbandService>()
                .AddServiceInterface<ICommandProducer, HookService>();

            return services;
        }

        public static IServiceCollection AddViewModels(this IServiceCollection services)
        {
            services = services ?? throw new ArgumentNullException(nameof(services));

            services
                .AddSingleton<MainViewModel>()
                .AddServiceInterface<IScreen, MainViewModel>();

            return services;
        }

        public static IServiceCollection AddServiceInterface<TInterface, TService>(this IServiceCollection services)
            where TService : class, TInterface
            where TInterface : class
        {
            services = services ?? throw new ArgumentNullException(nameof(services));

            services
                .AddSingleton<TInterface, TService>(provider => provider.GetRequiredService<TService>());

            return services;
        }
    }
}
