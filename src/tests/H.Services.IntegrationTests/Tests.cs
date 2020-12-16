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
            await using var runnerService = new RunnerService(moduleFinder, moduleService, recognitionService, deskbandService);
            runnerService.CallRunning += (_, call) =>
            {
                Console.WriteLine($"{nameof(runnerService.CallRunning)}: {call}");
            };
            runnerService.CallRan += (_, call) =>
            {
                Console.WriteLine($"{nameof(runnerService.CallRan)}: {call}");
            };
            runnerService.CallCancelled += (_, call) =>
            {
                Console.WriteLine($"{nameof(runnerService.CallCancelled)}: {call}");
            };
            var exceptions = new ExceptionsBag();
            foreach (var service in new IServiceBase[] { moduleService, recognitionService, moduleFinder, runnerService, deskbandService })
            {
                service.ExceptionOccurred += (_, exception) =>
                {
                    Console.WriteLine($"{nameof(service.ExceptionOccurred)}: {exception}");
                    exceptions.OnOccurred(exception);

                    // ReSharper disable once AccessToDisposedClosure
                    cancellationTokenSource.Cancel();
                };
            }
            foreach (var service in new ICommandProducer[] { moduleService, recognitionService, deskbandService })
            {
                service.CommandReceived += (_, value) =>
                {
                    Console.WriteLine($"{nameof(service.CommandReceived)}: {value}");
                };
                service.AsyncCommandReceived += (_, value, _) =>
                {
                    Console.WriteLine($"{nameof(service.AsyncCommandReceived)}: {value}");
                    
                    return Task.CompletedTask;
                };
            }

            moduleService.Add(new IpcClientServiceRunner("deskband", deskbandService));
            moduleService.Add(new RecognitionServiceRunner(recognitionService));

            await runnerService.StartRecord5SecondsStopRecordTestAsync(cancellationToken);

            await runnerService.RunAsync(new Command("run", "sleep", "5000"), cancellationToken);
            
            runnerService.CancelAll();

            await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);

            await runnerService.RunAsync(new Command("deskband", "clear-preview", ""), cancellationToken);
            
            exceptions.EnsureNoExceptions();
        }
    }
}
