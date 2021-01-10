using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ReactiveUI;
using Splat;
using Splat.Microsoft.Extensions.DependencyInjection;
using Splat.Microsoft.Extensions.Logging;
using H.Services.Apps.ViewModels;
using H.Services.Apps.Views;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace H.Services.Apps
{
    public static class InitializeMethods
    {
        public static IHost InitializeHost()
        {
#if __WASM__
            return new HostBuilder()
                .UseContentRoot(System.IO.Directory.GetCurrentDirectory())
#else
            return Host
              .CreateDefaultBuilder()
#endif
              .ConfigureServices(ConfigureServices)
              .ConfigureLogging(ConfigureLogging)
              .Build();
        }

        private static void ConfigureServices(IServiceCollection services)
        {
            services.UseMicrosoftDependencyResolver();

            var resolver = Locator.CurrentMutable;
            resolver.InitializeSplat();
            resolver.InitializeReactiveUI();

            // Register services.
            services
                .AddSingleton(_ => new StaticModuleService())
                .AddSingleton(provider => new RunnerService(provider.GetRequiredService<StaticModuleService>()));

            // Register main view model.
            services
                .AddSingleton<MainViewModel>()
                .AddSingleton<IScreen>(provider => provider.GetRequiredService<MainViewModel>());

            // Register other view models.
            services
                .AddSingleton<MainViewModel>();

            // Register views.
            services
                .AddTransient<IViewFor<MainViewModel>, MainPage>();
        }

        private static void ConfigureLogging(ILoggingBuilder builder)
        {
#if WINDOWS_UWP
            // Remove loggers incompatible with UWP.
            foreach (var logger in builder
              .Services
              .Where(service => service.ImplementationType == typeof(Microsoft.Extensions.Logging.EventLog.EventLogLoggerProvider))
              .ToList())
            {
              builder.Services.Remove(logger);
            }
#endif

            builder
                .AddSplat()
#if !__WASM__
                .AddConsole()
#else
                .ClearProviders()            
#endif

#if DEBUG
                .SetMinimumLevel(LogLevel.Debug)
#else
                .SetMinimumLevel(LogLevel.Information)
#endif
                ;
        }
    }
}
