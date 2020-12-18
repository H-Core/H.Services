using System;
using System.Linq;
using H.Core;
using H.Core.Runners;

namespace H.Services
{
    /// <summary>
    /// 
    /// </summary>
    public class RecognitionServiceRunner : Runner
    {
        #region Constructors

        /// <summary>
        /// 
        /// </summary>
        public RecognitionServiceRunner(RecognitionService service)
        {
            service = service ?? throw new ArgumentNullException(nameof(service));

            Add(new AsyncAction("start-recognition", service.StartAsync));
            Add(new AsyncAction("stop-recognition", service.StopAsync));
            
            Add(AsyncAction.WithCommand("record", async (command, cancellationToken) =>
            {
                var format = Enum.TryParse<RecordingFormat>(
                    command.Arguments.ElementAt(0), true, out var result)
                    ? result
                    : RecordingFormat.Mp3;
                var process = command.Process ?? throw new ArgumentException(nameof(command.Process));
                
                using var recording = await service.StartRecordAsync(format, cancellationToken)
                    .ConfigureAwait(false);

                await process.WaitAsync(cancellationToken).ConfigureAwait(false);

                await recording.StopAsync(cancellationToken).ConfigureAwait(false);
                
                return new Command(string.Empty, recording.Data);
            }));
        }

        #endregion
    }
}
