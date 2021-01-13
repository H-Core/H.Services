using System;
using System.Threading;
using System.Threading.Tasks;
using H.Core;
using H.Core.Runners;
using H.Services.Apps.ViewModels;
using H.Services.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Splat;

#pragma warning disable IDE0079
#pragma warning disable CA2000

namespace H.Services.Apps.Initialization
{
    /// <summary>
    /// 
    /// </summary>
    public static class InitializeHostExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="host"></param>
        /// <param name="traceAction"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
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
                        hookService.UpCombinationCaught += (_, _) =>
                        {
                            //traceAction?.Invoke($"{nameof(hookService.UpCombinationCaught)}: {value}");
                        };
                        hookService.DownCombinationCaught += (_, _) =>
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="host"></param>
        /// <returns></returns>
        public static IHost InitializeServiceRunners(this IHost host)
        {
            host = host ?? throw new ArgumentNullException(nameof(host));

            var staticModuleService = host.Services.GetRequiredService<StaticModuleService>();
            
            staticModuleService.Add(new DeskbandServiceRunner(host.Services.GetRequiredService<DeskbandService>()));
            staticModuleService.Add(new RecognitionServiceRunner(host.Services.GetRequiredService<RecognitionService>()));
            staticModuleService.Add(new CallNotifier(
                host.Services.GetRequiredService<RunnerService>(), 
                new Command("sound", "start-recording"), 
                "start-recognition"));
            staticModuleService.Add(new CallNotifier(
                host.Services.GetRequiredService<RunnerService>(),
                new Command("sound", "event"),
                "print",
                value => value.StartsWith("havendv:", StringComparison.OrdinalIgnoreCase)));

            return host;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="host"></param>
        /// <returns></returns>
        public static IHost InitializeViewModelsRunners(this IHost host)
        {
            host = host ?? throw new ArgumentNullException(nameof(host));

            var staticModuleService = host.Services.GetRequiredService<StaticModuleService>();
            var mainViewModel = host.Services.GetRequiredService<MainViewModel>();

            staticModuleService.Add(new Runner
            {
                SyncAction.WithSingleArgument("print", mainViewModel.WriteLine, "value"),
            });

            return host;
        }
    }
}
