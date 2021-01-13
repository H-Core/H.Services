using System;
using System.Threading;
using System.Threading.Tasks;
using H.Core;
using H.Services.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace H.Services.IntegrationTests
{
    [TestClass]
    public class DeskbandServiceTests
    {
        [TestMethod]
        public async Task SimpleTest()
        {
            using var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(15));
            var cancellationToken = cancellationTokenSource.Token;

            await using var deskbandService = new DeskbandService
            {
                ConnectedCommandFactory = _ => new Command("print", "Connected to H.DeskBand."),
                DisconnectedCommandFactory = _ => new Command("print", "Disconnected from H.DeskBand."),
            };
            await using var moduleService = new StaticModuleService(
                TestModules.CreateTimerNotifierWithDeskbandDateTimeEach1Seconds(),
                TestModules.CreateRunnerWithPrintCommand()
            );
            await using var runnerService = new RunnerService(
                moduleService, 
                moduleService, deskbandService
            );
            
            using var exceptions = new IServiceBase[]
            {
                moduleService, runnerService, deskbandService
            }.EnableLogging(cancellationTokenSource);

            moduleService.Add(new DeskbandServiceRunner(deskbandService));

            await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);

            try
            {
                await runnerService.RunAsync(
                    new Command("deskband", "clear-preview"),
                    cancellationToken);
            }
            catch (OperationCanceledException)
            {
                // ignore cancelling on Github Actions.
            }
        }
    }
}
