using System;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using H.Services.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace H.Services.IntegrationTests
{
    [TestClass]
    public class RecognitionServiceTests
    {
        [TestMethod]
        public async Task SimpleTest()
        {
            using var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(30));
            var cancellationToken = cancellationTokenSource.Token;

            await using var moduleService = new StaticModuleService(
                TestModules.CreateDefaultRecorder(),
                TestModules.CreateDefaultRecognizer()
            );
            await using var recognitionService = new RecognitionService(moduleService);
            
            using var exceptions = new IServiceBase[]
            {
                moduleService, recognitionService
            }.EnableLogging(cancellationTokenSource);

            await recognitionService.Start_Wait5Seconds_Stop_TestAsync(cancellationToken);
        }

        [TestMethod]
        public async Task TelegramTest()
        {
            using var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(30));
            var cancellationToken = cancellationTokenSource.Token;

            await using var container = IoCTests.CreateContainer(
                TestModules.CreateDefaultRecorder(),
                TestModules.CreateDefaultRecognizer(),
                TestModules.CreateTelegramRunner(),
                TestModules.CreateAliasRunnerCommand("telegram", "телеграмм", "отправь", "отправить")
            );
            using var exceptions = container.EnableLoggingForServices(cancellationTokenSource);

            await container.Resolve<RecognitionService>().Start_Wait5Seconds_Stop_TestAsync(cancellationToken);
        }
    }
}
