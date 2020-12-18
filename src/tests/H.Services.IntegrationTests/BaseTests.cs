using System;
using System.Threading;
using System.Threading.Tasks;
using H.Core;

namespace H.Services.IntegrationTests
{
    public static class BaseTests
    {
        public static async Task Start_Wait5Seconds_Stop_TestAsync(
            this RecognitionService service,
            CancellationToken cancellationToken = default)
        {
            using var recognition = await service.StartAsync(cancellationToken);

            await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);

            await recognition.StopAsync(cancellationToken);
        }
        
        public static async Task Start5SecondsStart5SecondsStopTestAsync(
            this RecognitionService service, 
            CancellationToken cancellationToken = default)
        {
            await service.StartAsync(cancellationToken);

            await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);

            await service.StartAsync(cancellationToken);

            await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);

            await service.StopAsync(cancellationToken);
        }
        
        public static async Task StartRecord5SecondsStopRecordTestAsync(
            this RunnerService service,
            CancellationToken cancellationToken = default)
        {
            await service.RunAsync(new Command("start-recognition"), cancellationToken);

            await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);

            await service.RunAsync(new Command("stop-recognition"), cancellationToken);
        }
    }
}
