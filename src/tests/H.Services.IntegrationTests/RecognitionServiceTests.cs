﻿using System;
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
            await using var finderService = new FinderService(moduleService);
            await using var recognitionService = new RecognitionService(finderService);
            await using var runnerService = new RunnerService(
                finderService, 
                moduleService, recognitionService
            );
            
            using var exceptions = new IServiceBase[]
            {
                moduleService, recognitionService, finderService, runnerService
            }.EnableLogging(cancellationTokenSource);

            moduleService.Add(new RecognitionServiceRunner(recognitionService));

            await runnerService.StartRecord5SecondsStopRecordTestAsync(cancellationToken);
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

            await container.Resolve<RecognitionService>().Start5SecondsStop1SecondTestAsync(cancellationToken);
        }
    }
}