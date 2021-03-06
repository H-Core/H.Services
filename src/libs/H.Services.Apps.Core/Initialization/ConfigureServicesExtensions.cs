﻿using System;
using System.Linq;
using H.Core;
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
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddModules(this IServiceCollection services)
        {
            services = services ?? throw new ArgumentNullException(nameof(services));

            services
                .AddTransient<IModule, NAudioRecorder>()
                .AddTransient<IModule, NAudioPlayer>()
                .AddTransient<IModule>(static _ => new WitAiRecognizer
                {
                    Token = "XZS4M3BUYV5LBMEWJKAGJ6HCPWZ5IDGY"
                })
                .AddTransient<IModule, YandexSynthesizer>()
                .AddTransient<IModule, GoogleSearcher>()
                .AddTransient<IModule>(static _ => new AliasRunner(
                    new Command("start-process", "https://github.com/new"), 
                    "start-project", "создай проект", "новый проект"))
                .AddTransient<IModule>(static _ => new AliasRunner("torrent", "смотреть"))
                .AddTransient<IModule>(static _ => new AliasRunner("telegram-message", "телеграмм", "отправь", "отправить"))
                .AddTransient<IModule>(static _ => new AliasRunner("say", "повтори", "повторить", "скажи"))
                .AddTransient<IModule>(static _ => new TelegramRunner
                {
                    Token = Environment.GetEnvironmentVariable("TELEGRAM_HOMECENTER_BOT_TOKEN")
                            ?? throw new InvalidOperationException("TELEGRAM_HOMECENTER_BOT_TOKEN environment variable is not found."),
                    DefaultUserId = 482553595,
                })
                .AddTransient<IModule, TorrentRunner>()
                .AddTransient<IModule, ProcessRunner>()
                .AddTransient<IModule, InternetRunner>()
                .AddTransient<IModule, UserRunner>()
                .AddTransient<IModule, SequenceRunner>()
                .AddTransient<IModule, ProcessRunner>()
                //.AddTransient<IModule, ScreenshotRunner>()
                .AddTransient<IModule, IntegrationRunner>()
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
                .AddTransient(static _ => new BoundCommand(
                    new Command("start-recognition"),
                    new Keys(Key.MouseMiddle, Key.MouseMiddle)
                ))
                //.AddTransient(static _ => new BoundCommand(
                //    new Command("start-telegram-voice-message"), 
                //    new Keys(Key.L, Key.RAlt)
                //    ))
                //.AddTransient(static _ => new BoundCommand(
                //    new Command(
                //        "process-sequence",
                //        "3",
                //        "select",
                //        "screenshot",
                //        "clipboard-set-image"
                //    ),
                //    new Keys(Key.RAlt), 
                //    true))
                ;

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
                    ConnectedCommandFactory = static _ => new Command("print", "Connected to H.DeskBand."),
                    DisconnectedCommandFactory = static _ => new Command("print", "Disconnected from H.DeskBand."),
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
                .AddInterface<IScreen, MainViewModel>()

                .AddSingleton<PreviewViewModel>();

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
                .AddSingleton<TInterface, TService>(static provider => provider.GetRequiredService<TService>());

            return services;
        }
    }
}
