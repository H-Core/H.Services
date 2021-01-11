using System;
using System.Threading;
using System.Threading.Tasks;
using H.Core;
using H.Services.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Splat;

namespace H.Services.Apps.Initialization
{
    public static class InitializeHostExtensions
    {
        public static async Task InitializeServicesAsync(
            this IHost host, 
            Action<string>? traceAction = null, 
            CancellationToken cancellationToken = default)
        {
            host = host ?? throw new ArgumentNullException(nameof(host));

            foreach (var service in host.Services.GetServices<IServiceBase>())
            {
                service.ExceptionOccurred += (_, value) =>
                {
                    traceAction?.Invoke($"{nameof(service.ExceptionOccurred)}: {value}");
                    LogHost.Default.Warn(value);
                };
                if (service is ICommandProducer commandProducer)
                {
                    commandProducer.CommandReceived += (_, value) =>
                    {
                        traceAction?.Invoke($"{nameof(commandProducer.CommandReceived)}: {value}");
                    };
                    commandProducer.AsyncCommandReceived += (_, value, _) =>
                    {
                        traceAction?.Invoke($"{nameof(commandProducer.AsyncCommandReceived)}: {value}");

                        return Task.FromResult<IValue>(Value.Empty);
                    };
                }
                switch (service)
                {
                    case HookService hookService:
                        hookService.UpCombinationCaught += (_, value) =>
                        {
                            //traceAction?.Invoke($"{nameof(hookService.UpCombinationCaught)}: {value}");
                        };
                        hookService.DownCombinationCaught += (_, value) =>
                        {
                            //traceAction?.Invoke($"{nameof(hookService.DownCombinationCaught)}: {value}");
                        };
                        break;

                    case RecognitionService recognitionService:
                        recognitionService.PreviewCommandReceived += (_, value) =>
                        {
                            traceAction?.Invoke($"{nameof(recognitionService.PreviewCommandReceived)}: {value}");
                        };
                        break;

                    case RunnerService runnerService:
                        runnerService.CallRunning += (_, call) =>
                        {
                            traceAction?.Invoke($"{nameof(runnerService.CallRunning)}: {call}");
                        };
                        runnerService.CallRan += (_, call) =>
                        {
                            traceAction?.Invoke($"{nameof(runnerService.CallRan)}: {call}");
                        };
                        runnerService.CallCancelled += (_, call) =>
                        {
                            traceAction?.Invoke($"{nameof(runnerService.CallCancelled)}: {call}");
                        };
                        runnerService.NotSupported += (_, command) =>
                        {
                            traceAction?.Invoke($"{nameof(runnerService.NotSupported)}: {command}");
                        };
                        break;
                }

                await service.InitializeAsync(cancellationToken).ConfigureAwait(false);
            }
        }

        public static void InitializeServiceRunners(this IHost host)
        {
            host = host ?? throw new ArgumentNullException(nameof(host));

            var staticModuleService = host.Services.GetRequiredService<StaticModuleService>();
            
            staticModuleService.Add(new DeskbandServiceRunner(host.Services.GetRequiredService<DeskbandService>()));
            staticModuleService.Add(new RecognitionServiceRunner(host.Services.GetRequiredService<RecognitionService>()));
        }
    }
}
