using System;
using System.Threading;
using System.Threading.Tasks;
using H.Core;
using H.Services.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace H.Services.IntegrationTests
{
    [TestClass]
    public class Tests
    {
        [TestMethod]
        public async Task RecognitionServiceTest()
        {
            using var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(30));
            var cancellationToken = cancellationTokenSource.Token;

            await using var deskbandService = new IpcClientService("H.Deskband")
            {
                ConnectedCommandFactory = _ => new Command("print", "Connected to H.DeskBand."),
                DisconnectedCommandFactory = _ => new Command("print", "Disconnected from H.DeskBand."),
            };
            await using var moduleService = new StaticModuleService(
                TestModules.CreateDefaultRecorder(),
                TestModules.CreateDefaultRecognizer(),
                TestModules.CreateRunnerWithPrintCommand(),
                TestModules.CreateRunnerWithSleepCommand(),
                TestModules.CreateRunnerWithSyncSleepCommand(),
                TestModules.CreateRunnerWithRunAsyncCommand(),
                TestModules.CreateTelegramRunner()
            );
            await using var moduleFinder = new ModuleFinder(moduleService);
            await using var recognitionService = new RecognitionService(moduleFinder);
            await using var runnerService = new RunnerService(
                moduleFinder, 
                moduleService, recognitionService, deskbandService
            );
            
            using var exceptions = new IServiceBase[]
            {
                moduleService, recognitionService, moduleFinder, runnerService, deskbandService
            }.EnableLogging(cancellationTokenSource);

            moduleService.Add(new IpcClientServiceRunner("deskband", deskbandService));
            moduleService.Add(new RecognitionServiceRunner(recognitionService));

            await runnerService.StartRecord5SecondsStopRecordTestAsync(cancellationToken);
        }
    }
}
