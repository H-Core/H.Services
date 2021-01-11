using System;
using System.Linq;
using H.Core;
using H.Services.Apps.ViewModels;
using H.Services.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ReactiveUI;
using Splat;
using Splat.Microsoft.Extensions.DependencyInjection;
using Splat.Microsoft.Extensions.Logging;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace H.Services.Apps.Initialization
{
    public static class HostBuilder
    {
        public static IHostBuilder Create()
        {
            return Host
              .CreateDefaultBuilder()
              .ConfigureServices(ConfigureServices)
              .ConfigureLogging(ConfigureLogging);
        }

        private static void ConfigureServices(IServiceCollection services)
        {
            services.UseMicrosoftDependencyResolver();
            
            var resolver = Locator.CurrentMutable;
            resolver.InitializeSplat();
            resolver.InitializeReactiveUI();

            // Register modules.

            // Register services.
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

            // Register main view model.
            services
                .AddSingleton<MainViewModel>()
                .AddServiceInterface<IScreen, MainViewModel>();

            // Register other view models.
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

        private static void ConfigureLogging(ILoggingBuilder builder)
        {
            builder
                .AddSplat()
#if DEBUG
                .SetMinimumLevel(LogLevel.Debug)
#else
                .SetMinimumLevel(LogLevel.Information)
#endif
                ;
        }
    }
}
