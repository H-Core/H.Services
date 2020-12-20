using System;
using System.Threading;
using System.Threading.Tasks;
using H.Core;
using H.Core.Utilities;
using H.Services.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace H.Services.IntegrationTests
{
    [TestClass]
    public class HookServiceTests
    {
        [TestMethod]
        public async Task SimpleTest()
        {
            using var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(30));
            var cancellationToken = cancellationTokenSource.Token;

            await using var service = new HookService();
            using var exceptions = new ExceptionsBag();
            service.EnableLogging(exceptions, cancellationTokenSource);

            await service.InitializeAsync(cancellationToken);
            
            await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
        }
        
        [TestMethod]
        public async Task ProcessTest()
        {
            using var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(30));
            var cancellationToken = cancellationTokenSource.Token;

            await using var hookService = new HookService();
            await using var moduleService = new StaticModuleService(
                TestModules.CreateProcessJobRunnerCommand()
            );
            await using var runnerService = new RunnerService(
                moduleService,
                moduleService,
                hookService
            );
            using var exceptions = new IServiceBase[]
            {
                moduleService, runnerService, hookService
            }.EnableLogging(cancellationTokenSource);

            await hookService.InitializeAsync(cancellationToken);
            
            hookService.While(new BoundCommand(new Command("process-job"), ConsoleKey.K));
            
            await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
        }

        [TestMethod]
        public async Task TelegramTest()
        {
            using var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(30));
            var cancellationToken = cancellationTokenSource.Token;

            await using var hookService = new HookService
            {
                new (
                    new Command("telegram", "Hello, World!"), 
                    ConsoleKey.L, alt: true),
            };
            await using var moduleService = new StaticModuleService(
                TestModules.CreateDefaultRecorder(),
                TestModules.CreateDefaultRecognizer(),
                TestModules.CreateTelegramRunner()
            );
            await using var runnerService = new RunnerService(
                moduleService,
                moduleService,
                hookService
            );
            using var exceptions = new IServiceBase[]
            {
                moduleService, runnerService, hookService
            }.EnableLogging(cancellationTokenSource);

            await hookService.InitializeAsync(cancellationToken);
            await runnerService.InitializeAsync(cancellationToken);

            await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
        }

        [TestMethod]
        public async Task SendTelegramVoiceMessageTest()
        {
            using var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(30));
            var cancellationToken = cancellationTokenSource.Token;

            await using var hookService = new HookService();
            await using var moduleService = new StaticModuleService(
                TestModules.CreateDefaultRecorder(),
                TestModules.CreateDefaultRecognizer(),
                TestModules.CreateTelegramRunner()
            );
            await using var recognitionService = new RecognitionService(moduleService);
            await using var runnerService = new RunnerService(
                moduleService,
                moduleService,
                hookService
            );
            using var exceptions = new IServiceBase[]
            {
                moduleService, runnerService, hookService
            }.EnableLogging(cancellationTokenSource);

            await hookService.InitializeAsync(cancellationToken);

            moduleService.Add(new RecognitionServiceRunner(recognitionService));
            
            hookService.While(new BoundCommand(new Command("send-telegram-voice-message"), ConsoleKey.L, alt: true));

            await Task.Delay(TimeSpan.FromSeconds(15), cancellationToken);
        }
    }
}
