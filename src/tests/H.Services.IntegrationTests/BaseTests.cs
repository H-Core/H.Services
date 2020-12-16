using System;
using System.Threading;
using System.Threading.Tasks;
using H.Core;

namespace H.Services.IntegrationTests
{
    public static class BaseTests
    {
        public static async Task Start5SecondsStart5SecondsStopTestAsync(
            this RecognitionService service, 
            CancellationToken cancellationToken = default)
        {
            await service.StartAsync(cancellationToken);

            await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);

            await service.StartAsync(cancellationToken);

            await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);

            await service.StopAsync(cancellationToken);

            try
            {
                await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
            }
            catch (OperationCanceledException)
            {
            }
        }
        
        public static async Task StartRecord5SecondsStopRecordTestAsync(
            this RunnerService service,
            CancellationToken cancellationToken = default)
        {
            await service.RunAsync(new Command("start-recognition"), cancellationToken);

            await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);

            await service.RunAsync(new Command("stop-recognition"), cancellationToken);

            try
            {
                await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
            }
            catch (OperationCanceledException)
            {
            }
        }
    }
}
