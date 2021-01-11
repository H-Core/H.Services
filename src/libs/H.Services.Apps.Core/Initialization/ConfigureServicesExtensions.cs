using System;
using System.Linq;
using H.Core;
using H.Core.Runners;
using H.Recognizers;
using H.Recorders;
using H.Runners;
using H.Searchers;
using H.Services.Apps.ViewModels;
using H.Services.Core;
using H.Synthesizers;
using Microsoft.Extensions.DependencyInjection;
using ReactiveUI;

namespace H.Services.Apps.Initialization
{
    public static class ConfigureServicesExtensions
    {
        private static IModule CreateAliasRunner(string name, params string[] aliases)
        {
            var runner = new Runner();
            foreach (var alias in aliases)
            {
                runner.Add(SyncAction.WithCommand(
                    alias,
                    command => runner.Run(new Command(name, command.Input.Arguments)),
                    "arguments"));
            }

            return runner;
        }

        public static IServiceCollection AddModules(this IServiceCollection services)
        {
            services = services ?? throw new ArgumentNullException(nameof(services));

            services
                .AddSingleton<IModule, NAudioRecorder>()
                .AddSingleton<IModule, NAudioPlayer>()
                .AddSingleton<IModule>(new WitAiRecognizer
                {
                    Token = "XZS4M3BUYV5LBMEWJKAGJ6HCPWZ5IDGY"
                })
                .AddSingleton<IModule, YandexSynthesizer>()
                .AddSingleton<IModule, GoogleSearcher>()
                .AddSingleton(CreateAliasRunner("torrent", "смотреть"))
                .AddSingleton(CreateAliasRunner("telegram message", "телеграмм", "отправь", "отправить"))
                .AddSingleton(CreateAliasRunner("say", "повтори", "повторить", "скажи"))
                .AddSingleton<IModule>(new TelegramRunner
                {
                    Token = Environment.GetEnvironmentVariable("TELEGRAM_HOMECENTER_BOT_TOKEN")
                            ?? throw new InvalidOperationException("TELEGRAM_HOMECENTER_BOT_TOKEN environment variable is not found."),
                    DefaultUserId = 482553595,
                })
                .AddSingleton<IModule, TorrentRunner>()
                .AddSingleton<IModule, IntegrationRunner>()
                ;

            return services;
        }

        public static IServiceCollection AddBoundCommands(this IServiceCollection services)
        {
            services = services ?? throw new ArgumentNullException(nameof(services));

            services
                .AddSingleton(new BoundCommand(
                    new Command("send-telegram-voice-message"), 
                    ConsoleKey.L, control: true, isProcessing: true));

            return services;
        }

        public static IServiceCollection AddServices(this IServiceCollection services)
        {
            services = services ?? throw new ArgumentNullException(nameof(services));

            services
                .AddSingleton(static provider =>
                    new StaticModuleService(
                        provider.GetServices<IModule>().ToArray()))
                .AddInterface<IServiceBase, StaticModuleService>()
                .AddInterface<IModuleService, StaticModuleService>()
                .AddInterface<ICommandProducer, StaticModuleService>()

                .AddSingleton(static provider =>
                    new RecognitionService(
                        provider.GetServices<IModuleService>().ToArray()))
                .AddInterface<IServiceBase, RecognitionService>()
                .AddInterface<ICommandProducer, RecognitionService>()

                .AddSingleton(static provider => 
                    new RunnerService(
                        provider.GetServices<IModuleService>().ToArray(), 
                        provider.GetServices<ICommandProducer>().ToArray()))
                .AddInterface<IServiceBase, RunnerService>()

                .AddSingleton(static provider =>
                    new HookService(
                        provider.GetServices<BoundCommand>().ToArray()))
                .AddInterface<IServiceBase, HookService>()
                .AddInterface<ICommandProducer, HookService>()
                .AddInterface<IProcessCommandProducer, HookService>()

                .AddSingleton(static _ => new DeskbandService
                {
                    ConnectedCommandFactory = _ => new Command("print", "Connected to H.DeskBand."),
                    DisconnectedCommandFactory = _ => new Command("print", "Disconnected from H.DeskBand."),
                })
                .AddInterface<IServiceBase, DeskbandService>()
                .AddInterface<ICommandProducer, DeskbandService>()
                ;

            return services;
        }

        public static IServiceCollection AddViewModels(this IServiceCollection services)
        {
            services = services ?? throw new ArgumentNullException(nameof(services));

            services
                .AddSingleton<MainViewModel>()
                .AddInterface<IScreen, MainViewModel>();

            return services;
        }

        public static IServiceCollection AddInterface<TInterface, TService>(this IServiceCollection services)
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
