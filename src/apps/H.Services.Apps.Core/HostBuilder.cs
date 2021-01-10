using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ReactiveUI;
using Splat;
using Splat.Microsoft.Extensions.DependencyInjection;
using H.Services.Apps.ViewModels;
using Splat.Microsoft.Extensions.Logging;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace H.Services.Apps
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

            // Register services.
            services
                .AddSingleton(_ => new StaticModuleService())
                .AddSingleton(provider => new RunnerService(provider.GetRequiredService<StaticModuleService>()));

            // Register main view model.
            services
                .AddSingleton<MainViewModel>()
                .AddSingleton<IScreen, MainViewModel>(provider => provider.GetRequiredService<MainViewModel>());

            // Register other view models.
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
