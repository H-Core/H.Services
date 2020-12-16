using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using H.Core.Utilities;
using H.Services.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace H.Services.IntegrationTests
{
    [TestClass]
    public class IoCTests
    {

        public static IContainer CreateContainer()
        {
            var builder = new ContainerBuilder();

            builder
                .RegisterInstance(TestModules.CreateDefaultRecorder())
                .AsImplementedInterfaces();
            builder
                .RegisterInstance(TestModules.CreateDefaultRecognizer())
                .AsImplementedInterfaces();
            builder
                .RegisterInstance(TestModules.CreateTimerNotifierWithPrintHelloWorldEach3Seconds())
                .AsImplementedInterfaces();
            builder
                .RegisterInstance(TestModules.CreateRunnerWithPrintCommand())
                .AsImplementedInterfaces();
            builder
                .RegisterInstance(TestModules.CreateTelegramRunner())
                .AsImplementedInterfaces();
            builder
                .RegisterType<StaticModuleService>()
                .SingleInstance()
                .AsImplementedInterfaces()
                .AsSelf();
            builder
                .RegisterType<ModuleFinder>()
                .SingleInstance()
                .AsImplementedInterfaces()
                .AsSelf();
            builder
                .RegisterType<RecognitionService>()
                .SingleInstance()
                .AsImplementedInterfaces()
                .AsSelf();
            builder
                .RegisterType<RunnerService>()
                .SingleInstance()
                .AsImplementedInterfaces()
                .AsSelf();

            return builder.Build();
        }

        [TestMethod]
        public async Task RecognitionServiceTest()
        {
            using var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(30));
            var cancellationToken = cancellationTokenSource.Token;

            await using var container = CreateContainer();
            var exceptions = new ExceptionsBag();
            
            foreach (var service in container.Resolve<IEnumerable<IServiceBase>>())
            {
                service.ExceptionOccurred += (_, exception) =>
                {
                    Console.WriteLine($"{nameof(service.ExceptionOccurred)}: {exception}");
                    exceptions.OnOccurred(exception);

                    // ReSharper disable once AccessToDisposedClosure
                    cancellationTokenSource.Cancel();
                };
            }
            foreach (var service in container.Resolve<IEnumerable<ICommandProducer>>())
            {
                service.CommandReceived += (_, value) =>
                {
                    Console.WriteLine($"{nameof(service.CommandReceived)}: {value}");
                };
            }
            
            var recognitionService = container.Resolve<RecognitionService>();
            
            await recognitionService.Start5SecondsStart5SecondsStopTestAsync(cancellationToken);

            exceptions.EnsureNoExceptions();
        }
    }
}
