using System;
using System.Threading;
using System.Threading.Tasks;
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
        }
    }
}
