using System;
using System.Threading;
using System.Threading.Tasks;
using H.Core;
using H.IO.Utilities;
using H.Runners;
using H.Services.Core;
using H.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace H.Services.IntegrationTests
{
    [TestClass]
    public class RunnerServiceTests
    {
        [TestMethod]
        public async Task LongJobTest()
        {
            using var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(30));
            var cancellationToken = cancellationTokenSource.Token;

            await using var moduleService = new StaticModuleService(
                TestModules.CreateLongJobRunnerCommand()
            );
            await using var runnerService = new RunnerService(moduleService);

            using var exceptions = new IServiceBase[]
            {
                moduleService, runnerService
            }.EnableLogging(cancellationTokenSource);

            var values = await runnerService.RunAsync(
                new Command("long-job", "5000"), cancellationToken);
            
            Assert.AreEqual(1, values.Length);
            Assert.AreEqual("5000", values[0].Input.Argument);
        }
        
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

            var recognition = await recognitionService.StartAsync(cancellationToken);
            var bytes = await recognitionService.StartRecordMp3_5Second_Stop_Async(cancellationToken);
            var preview = await recognition.StopAsync(cancellationToken);

            await runnerService.RunAsync(
                new Command("telegram audio", new Value(string.Empty, preview)
                {
                    Data = bytes,
                }), cancellationToken);
        }

        [TestMethod]
        public async Task RussianNameTest()
        {
            using var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(30));
            var cancellationToken = cancellationTokenSource.Token;

            using var app = await TestWpfApp.CreateAsync(cancellationToken);
            await using var moduleService = new StaticModuleService(
                new AliasRunner(
                    new Command("sequence", "2", "clipboard-set-text", "keyboard CTRL+V"),
                    "вставь"),
                new KeyboardRunner(),
                new ClipboardRunner(app.Dispatcher),
                new SequenceRunner()
            );
            await using var runnerService = new RunnerService(moduleService, moduleService);
            
            using var exceptions = new IServiceBase[]
            {
                moduleService, runnerService
            }.EnableLogging(cancellationTokenSource);
            
            await runnerService.RunAsync(Command.Parse("вставь 123"), cancellationToken);
        }

        [TestMethod]
        public async Task SelectScreenshotClipboardTest()
        {
            using var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(30));
            var cancellationToken = cancellationTokenSource.Token;

            using var app = await TestWpfApp.CreateAsync(cancellationToken);
            await using var moduleService = new StaticModuleService(
                new SelectRunner(app.Dispatcher),
                new ScreenshotRunner(),
                new ClipboardRunner(app.Dispatcher)
            );
            await using var runnerService = new RunnerService(
                moduleService,
                moduleService
            );
            using var exceptions = new IServiceBase[]
            {
                moduleService, runnerService
            }.EnableLogging(cancellationTokenSource);

            moduleService.Add(new ProcessSequenceRunner(runnerService));

            var process = runnerService.Start(new Command(
                "process-sequence",
                "3",
                "select",
                "screenshot",
                "clipboard-set-image"
            ), cancellationToken);

            await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);

            await process.StopAsync(cancellationToken);

            await runnerService.WaitAllAsync(cancellationToken);
        }
    }
}
