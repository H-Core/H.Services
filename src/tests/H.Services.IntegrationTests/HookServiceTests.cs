﻿using System;
using System.Threading;
using System.Threading.Tasks;
using H.Core;
using H.Core.Utilities;
using H.Runners;
using H.Services.Core;
using H.Tests;
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

            await using var hookService = new HookService
            {
                new (new Command("process-job"), ConsoleKey.K, isProcessing: true),
            };
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
            
            await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
        }

        [TestMethod]
        public async Task SelectTest()
        {
            using var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(30));
            var cancellationToken = cancellationTokenSource.Token;

            using var app = await TestWpfApp.CreateAsync(cancellationToken);
            await using var hookService = new HookService
            {
                new (new Command("select"), ConsoleKey.S, isProcessing: true),
            };
            await using var moduleService = new StaticModuleService(
               new SelectRunner(app.Dispatcher)
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

            await Task.Delay(TimeSpan.FromSeconds(15), cancellationToken);
            
            await runnerService.WaitAllAsync(cancellationToken);
        }

        [TestMethod]
        public async Task SelectScreenshotClipboardTest()
        {
            using var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(30));
            var cancellationToken = cancellationTokenSource.Token;

            using var app = await TestWpfApp.CreateAsync(cancellationToken);
            await using var hookService = new HookService
            {
                new (new Command(
                    "process-sequence", 
                    "3", 
                    "select",
                    "screenshot",
                    "clipboard-set-image"
                    ),
                    new Keys(Key.RAlt),
                    true),
            };
            await using var moduleService = new StaticModuleService(
                new SelectRunner(app.Dispatcher),
                new ScreenshotRunner(),
                new ClipboardRunner(app.Dispatcher)
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

            moduleService.Add(new ProcessSequenceRunner(runnerService));

            await hookService.InitializeAsync(cancellationToken);

            await Task.Delay(TimeSpan.FromSeconds(15), cancellationToken);

            await runnerService.WaitAllAsync(cancellationToken);
        }

        [TestMethod]
        public async Task TelegramTest()
        {
            using var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(30));
            var cancellationToken = cancellationTokenSource.Token;

            await using var hookService = new HookService
            {
                new (
                    new Command("telegram-message", "Hello, World!"), 
                    new Keys(Key.L, Key.RAlt)),
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

            await using var hookService = new HookService
            {
                new (new Command("send-telegram-voice-message"), 
                    new Keys(Key.L, Key.RAlt),
                    true),
            };
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
            
            await Task.Delay(TimeSpan.FromSeconds(15), cancellationToken);
        }
    }
}
