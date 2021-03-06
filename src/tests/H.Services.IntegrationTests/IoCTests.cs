﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Autofac.Builder;
using H.Core;
using H.Services.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace H.Services.IntegrationTests
{
    [TestClass]
    public class IoCTests
    {
        private static void AddModule<T>(ContainerBuilder builder, T value) where T : class
        {
            builder
                .RegisterInstance(value)
                .AsImplementedInterfaces();
        }
        
        private static IRegistrationBuilder<T, ConcreteReflectionActivatorData, SingleRegistrationStyle> AddService<T>(
            ContainerBuilder builder) where T : notnull
        {
            return builder
                .RegisterType<T>()
                .SingleInstance()
                .AsImplementedInterfaces()
                .AsSelf();
        }

        public static IContainer CreateContainer(params IModule[] modules)
        {
            var builder = new ContainerBuilder();

            foreach (var module in modules)
            {
                AddModule(builder, module);
            }
            
            AddService<StaticModuleService>(builder);
            AddService<RecognitionService>(builder);
            AddService<RunnerService>(builder)
                .UsingConstructor(typeof(IModuleService[]), typeof(ICommandProducer[]));
            AddService<HookService>(builder);

            builder
                .RegisterInstance(new DeskbandService
                {
                    ConnectedCommandFactory = _ => new Command("print", "Connected to H.DeskBand."),
                    DisconnectedCommandFactory = _ => new Command("print", "Disconnected from H.DeskBand."),
                })
                .SingleInstance()
                .AsImplementedInterfaces()
                .AsSelf();

            return builder.Build();
        }

        [TestMethod]
        public async Task BaseTest()
        {
            using var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(30));
            var cancellationToken = cancellationTokenSource.Token;

            await using var container = CreateContainer(
                TestModules.CreateDefaultRecorder(),
                TestModules.CreateDefaultRecognizer(),
                TestModules.CreateRunnerWithPrintCommand()
            );
            using var exceptions = container.EnableLoggingForServices(cancellationTokenSource);
            
            var recognitionService = container.Resolve<RecognitionService>();
            
            await recognitionService.Start5SecondsStart5SecondsStopTestAsync(cancellationToken);
        }
    }
}
