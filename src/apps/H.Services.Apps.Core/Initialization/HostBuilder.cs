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

            services
                .AddModules()
                .AddServices()
                .AddViewModels();
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
