﻿using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using H.Core.Utilities;
using H.Services.Core;

namespace H.Services.IntegrationTests
{
    public static class ConsoleLoggerExtensions
    {
        public static void EnableLogging(
            this IServiceBase service, 
            ExceptionsBag? exceptions = null, 
            CancellationTokenSource? source = null)
        {
            service.ExceptionOccurred += (_, exception) =>
            {
                Console.WriteLine($"{nameof(service.ExceptionOccurred)}: {exception}");
                exceptions?.OnOccurred(exception);
                source?.Cancel();
            };
            if (service is ICommandProducer commandProducer)
            {
                commandProducer.CommandReceived += (_, value) =>
                {
                    Console.WriteLine($"{nameof(commandProducer.CommandReceived)}: {value}");
                };
                commandProducer.AsyncCommandReceived += (_, value, _) =>
                {
                    Console.WriteLine($"{nameof(commandProducer.AsyncCommandReceived)}: {value}");

                    return Task.CompletedTask;
                };
            }
            if (service is HookService hookService)
            {
                hookService.CombinationCaught += (_, value) =>
                {
                    Console.WriteLine($"{nameof(hookService.CombinationCaught)}: {value}");
                };
            }
            if (service is RecognitionService recognitionService)
            {
                recognitionService.PreviewCommandReceived += (_, value) =>
                {
                    Console.WriteLine($"{nameof(recognitionService.PreviewCommandReceived)}: {value}");
                };
            }
            if (service is RunnerService runnerService)
            {
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
            }
        }

        public static ExceptionsBag EnableLogging(
            this IEnumerable<IServiceBase> services,
            CancellationTokenSource? source = null)
        {
            var exceptions = new ExceptionsBag();
            foreach (var service in services)
            {
                service.EnableLogging(exceptions, source);
            }

            return exceptions;
        }

        public static ExceptionsBag EnableLoggingForServices(
            this IContainer container,
            CancellationTokenSource? source = null)
        {
            return container
                .Resolve<IEnumerable<IServiceBase>>()
                .EnableLogging(source);
        }
    }
}