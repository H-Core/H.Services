using System;
using System.Windows;
using H.Core;
using H.Runners;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ReactiveUI;
using H.Services.Apps.ViewModels;
using H.Services.Apps.Views;

namespace H.Services.Apps
{
    public static class HostBuilderExtensions
    {
        public static IHostBuilder AddViews(this IHostBuilder hostBuilder)
        {
            hostBuilder = hostBuilder ?? throw new ArgumentNullException(nameof(hostBuilder));

            hostBuilder.ConfigureServices(static services =>
            {
                services
                    .AddTransient<IViewFor<MainViewModel>, MainWindow>()
                    .AddTransient<IViewFor<PreviewViewModel>, PreviewWindow>();
            });

            return hostBuilder;
        }

        public static IHostBuilder AddPlatformSpecificModules(this IHostBuilder hostBuilder)
        {
            hostBuilder = hostBuilder ?? throw new ArgumentNullException(nameof(hostBuilder));

            hostBuilder.ConfigureServices(static services =>
            {
                services
                    .AddSingleton(_ => Application.Current.Dispatcher)
                    .AddTransient<IModule, ClipboardRunner>()
                    .AddTransient<IModule, SelectRunner>()
                    .AddTransient<IModule, KeyboardRunner>()
                    .AddTransient<IModule, SoundRunner>()
                    .AddTransient<IModule>(_ => new AliasRunner(
                        new Command("sequence", "2", "clipboard-set", "keyboard ^v"),
                        "paste"))
                    .AddTransient<IModule>(_ => new AliasRunner("paste", "вставь"))
                    ;
            });

            return hostBuilder;
        }

        public static IHostBuilder AddPlatformSpecificLoggers(this IHostBuilder hostBuilder)
        {
            hostBuilder = hostBuilder ?? throw new ArgumentNullException(nameof(hostBuilder));

            hostBuilder.ConfigureLogging(static builder =>
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
#if __WASM__
                    .ClearProviders()
#else
                    .AddConsole()
#endif
                    ;
            });

            return hostBuilder;
        }
    }
}
