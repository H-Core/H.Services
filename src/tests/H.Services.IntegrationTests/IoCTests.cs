using System;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
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
        
        private static void AddService<T>(ContainerBuilder builder) where T : notnull
        {
            builder
                .RegisterType<T>()
                .SingleInstance()
                .AsImplementedInterfaces()
                .AsSelf();
        }

        public static IContainer CreateContainer()
        {
            var builder = new ContainerBuilder();

            AddModule(builder, TestModules.CreateDefaultRecorder());
            AddModule(builder, TestModules.CreateDefaultRecognizer());
            AddModule(builder, TestModules.CreateRunnerWithPrintCommand());
            AddModule(builder, TestModules.CreateTelegramRunner());
            
            AddService<StaticModuleService>(builder);
            AddService<ModuleFinder>(builder);
            AddService<RecognitionService>(builder);
            AddService<RunnerService>(builder);

            return builder.Build();
        }

        [TestMethod]
        public async Task RecognitionServiceTest()
        {
            using var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(30));
            var cancellationToken = cancellationTokenSource.Token;

            await using var container = CreateContainer();
            using var exceptions = container.EnableLoggingForServices(cancellationTokenSource);
            
            var recognitionService = container.Resolve<RecognitionService>();
            
            await recognitionService.Start5SecondsStart5SecondsStopTestAsync(cancellationToken);
        }
    }
}
