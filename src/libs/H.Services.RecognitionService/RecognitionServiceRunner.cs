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
            
            Add(new ProcessAction("record", async (process, command, cancellationToken) =>
            {
                var format = Enum.TryParse<RecordingFormat>(
                    command.Value.Arguments.ElementAt(0), true, out var result)
                    ? result
                    : RecordingFormat.Mp3;
                
                using var recording = await service.StartRecordAsync(format, cancellationToken)
                    .ConfigureAwait(false);

                await process.WaitAsync(cancellationToken).ConfigureAwait(false);

                await recording.StopAsync(cancellationToken).ConfigureAwait(false);
                
                return new Value(recording.Data);
            }));
        }

        #endregion
    }
}
