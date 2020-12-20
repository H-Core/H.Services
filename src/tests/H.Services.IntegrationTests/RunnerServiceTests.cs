using System;
using System.Threading;
using System.Threading.Tasks;
using H.Core;
using H.IO.Utilities;
using H.Services.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace H.Services.IntegrationTests
{
    [TestClass]
    public class RunnerServiceTests
    {
        [TestMethod]
        public async Task TelegramMessageTest()
        {
            using var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(30));
            var cancellationToken = cancellationTokenSource.Token;

            await using var moduleService = new StaticModuleService(
                TestModules.CreateTelegramRunner()
            );
            await using var runnerService = new RunnerService(moduleService);

            using var exceptions = new IServiceBase[]
            {
                moduleService, runnerService
            }.EnableLogging(cancellationTokenSource);

            await runnerService.RunAsync(
                new Command("telegram message", nameof(TelegramMessageTest)), cancellationToken);
            
            // TODO: bug or?
            await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
        }
        
        [TestMethod]
        public async Task TelegramAudioTest()
        {
            using var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(30));
            var cancellationToken = cancellationTokenSource.Token;

            await using var moduleService = new StaticModuleService(
                TestModules.CreateTelegramRunner()
            );
            await using var runnerService = new RunnerService(moduleService);

            using var exceptions = new IServiceBase[]
            {
                moduleService, runnerService
            }.EnableLogging(cancellationTokenSource);

            await runnerService.RunAsync(
                new Command("telegram audio", new Value(string.Empty, nameof(TelegramAudioTest))
                {
                    Data = ResourcesUtilities.ReadFileAsBytes("test.mp3"),
                }), cancellationToken);

            // TODO: bug or?
            await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
        }

        [TestMethod]
        public async Task TelegramRecordedAudioTest()
        {
            using var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(30));
            var cancellationToken = cancellationTokenSource.Token;

            await using var moduleService = new StaticModuleService(
                TestModules.CreateDefaultRecorder(),
                TestModules.CreateTelegramRunner()
            );
            await using var runnerService = new RunnerService(moduleService);
            await using var recognitionService = new RecognitionService(moduleService);

            using var exceptions = new IServiceBase[]
            {
                moduleService, runnerService
            }.EnableLogging(cancellationTokenSource);

            var bytes = await recognitionService.StartRecordMp3_5Second_Stop_Async(cancellationToken);
            await runnerService.RunAsync(
                new Command("telegram audio", new Value(string.Empty, nameof(TelegramRecordedAudioTest))
                {
                    Data = bytes,
                }), cancellationToken);

            // TODO: bug or?
            await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
        }

        [TestMethod]
        public async Task TelegramRecordedAndConvertedAudioTest()
        {
            using var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(30));
            var cancellationToken = cancellationTokenSource.Token;

            await using var moduleService = new StaticModuleService(
                TestModules.CreateDefaultRecorder(),
                TestModules.CreateDefaultRecognizer(),
                TestModules.CreateTelegramRunner()
            );
            await using var runnerService = new RunnerService(moduleService);
            await using var recognitionService = new RecognitionService(moduleService);

            using var exceptions = new IServiceBase[]
            {
                moduleService, runnerService
            }.EnableLogging(cancellationTokenSource);

            var recording = await recognitionService.StartRecordAsync(RecordingFormat.Wav, cancellationToken);
            var bytes = await recognitionService.StartRecordMp3_5Second_Stop_Async(cancellationToken);
            await recording.StopAsync(cancellationToken);
            var preview = await recognitionService.ConvertAsync(recording.Data, cancellationToken);
            await runnerService.RunAsync(
                new Command("telegram audio", new Value(string.Empty, preview, "412536036")
                {
                    Data = bytes,
                }), cancellationToken);

            // TODO: bug or?
            await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
        }
    }
}
