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
        public async Task TelegramTest()
        {
            using var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(30));
            var cancellationToken = cancellationTokenSource.Token;

            await using var hookService = new HookService
            {
                new (
                    new Command("telegram", "Hello, World!"), 
                    ConsoleKey.L, false, true, false),
            };
            await using var moduleService = new StaticModuleService(
                TestModules.CreateDefaultRecorder(),
                TestModules.CreateDefaultRecognizer(),
                TestModules.CreateTelegramRunner()
            );
            await using var finderService = new FinderService(moduleService);
            await using var runnerService = new RunnerService(
                finderService,
                moduleService,
                hookService
            );
            using var exceptions = new IServiceBase[]
            {
                moduleService, finderService, runnerService, hookService
            }.EnableLogging(cancellationTokenSource);

            await hookService.InitializeAsync(cancellationToken);
            await runnerService.InitializeAsync(cancellationToken);

            await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
        }
    }
}
