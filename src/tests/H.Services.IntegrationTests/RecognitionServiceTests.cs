using System;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using H.Core;
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
        public async Task RecordTest()
        {
            using var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(30));
            var cancellationToken = cancellationTokenSource.Token;

            await using var moduleService = new StaticModuleService(
                TestModules.CreateDefaultRecorder()
            );
            await using var recognitionService = new RecognitionService(moduleService);
            await using var runnerService = new RunnerService(moduleService, recognitionService);

            using var exceptions = new IServiceBase[]
            {
                moduleService, recognitionService, runnerService
            }.EnableLogging(cancellationTokenSource);

            moduleService.Add(new RecognitionServiceRunner(recognitionService));
            
            var process = runnerService.Start(new Command("record", "mp3"), cancellationToken);

            await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);

            var value = await process.StopAsync(cancellationToken);
            
            Assert.AreNotEqual(0, value.Output.Data.Length);
        }

        [TestMethod]
        public async Task SendTelegramVoiceMessageTest()
        {
            using var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(30));
            var cancellationToken = cancellationTokenSource.Token;

            await using var moduleService = new StaticModuleService(
                TestModules.CreateDefaultRecorder(),
                TestModules.CreateDefaultRecognizer(),
                TestModules.CreateTelegramRunner()
            );
            await using var recognitionService = new RecognitionService(moduleService);
            await using var runnerService = new RunnerService(moduleService, moduleService, recognitionService);

            using var exceptions = new IServiceBase[]
            {
                moduleService, recognitionService, runnerService
            }.EnableLogging(cancellationTokenSource);

            moduleService.Add(new RecognitionServiceRunner(recognitionService));

            var process = runnerService.Start(new Command("send-telegram-voice-message"), cancellationToken);

            await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
            
            var value = await process.StopAsync(cancellationToken);

            Assert.AreNotEqual(0, value.Output.Data.Length);
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

        [TestMethod]
        public async Task RepeatTest()
        {
            using var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(30));
            var cancellationToken = cancellationTokenSource.Token;

            await using var container = IoCTests.CreateContainer(
                TestModules.CreateDefaultRecorder(),
                TestModules.CreateDefaultRecognizer(),
                TestModules.CreateDefaultSynthesizer(),
                TestModules.CreateDefaultPlayer(),
                new IntegrationRunner(),
                TestModules.CreateAliasRunnerCommand("say", "повтори", "повторить", "скажи")
            );
            using var exceptions = container.EnableLoggingForServices(cancellationTokenSource);

            await container.Resolve<RecognitionService>().Start_Wait5Seconds_Stop_TestAsync(cancellationToken);

            await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
        }
    }
}
