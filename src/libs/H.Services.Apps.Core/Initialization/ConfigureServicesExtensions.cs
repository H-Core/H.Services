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

#pragma warning disable IDE0079
#pragma warning disable CA2000

namespace H.Services.Apps.Initialization
{
    /// <summary>
    /// 
    /// </summary>
    public static class ConfigureServicesExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="command"></param>
        /// <param name="aliases"></param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <returns></returns>
        public static IModule CreateAliasRunner(ICommand command, params string[] aliases)
        {
            command = command ?? throw new ArgumentNullException(nameof(command));

            var runner = new Runner();
            foreach (var alias in aliases)
            {
                runner.Add(SyncAction.WithCommand(
                    alias,
                    originalCommand =>
                    {
                        runner.Run(new Command(command.Name, new Value(
                            command.Input.Data.Concat(originalCommand.Input.Data).ToArray(),
                            command.Input.Arguments.Concat(originalCommand.Input.Arguments).ToArray())));
                    },
                    "arguments"));
            }

            return runner;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="aliases"></param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <returns></returns>
        public static IModule CreateAliasRunner(string name, params string[] aliases)
        {
            name = name ?? throw new ArgumentNullException(nameof(name));

            return CreateAliasRunner(new Command(name), aliases);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
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
                .AddTransient<IModule, InternetRunner>()
                .AddTransient<IModule, UserRunner>()
                .AddTransient<IModule, ProcessRunner>()
                .AddSingleton<IModule, IntegrationRunner>()
                ;

            return services;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddBoundCommands(this IServiceCollection services)
        {
            services = services ?? throw new ArgumentNullException(nameof(services));

            services
                .AddSingleton(new BoundCommand(
                    new Command("send-telegram-voice-message"), 
                    ConsoleKey.L, control: true, isProcessing: true));

            return services;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddViewModels(this IServiceCollection services)
        {
            services = services ?? throw new ArgumentNullException(nameof(services));

            services
                .AddSingleton<MainViewModel>()
                .AddInterface<IScreen, MainViewModel>();

            return services;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TInterface"></typeparam>
        /// <typeparam name="TService"></typeparam>
        /// <param name="services"></param>
        /// <returns></returns>
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
