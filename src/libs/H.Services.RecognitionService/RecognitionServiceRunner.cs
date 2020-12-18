using System;
using System.Globalization;
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
            Add(AsyncAction.WithCommand("start-record", async (command, cancellationToken) =>
            {
                var format = Enum.TryParse<RecordingFormat>(
                    command.Arguments.ElementAt(0), 
                    true, 
                    out var result)
                    ? result
                    : RecordingFormat.Mp3;
                var milliseconds = Convert.ToInt32(command.Arguments.ElementAt(1), CultureInfo.InvariantCulture);
                
                var bytes = await service.StartRecordAsync(
                        TimeSpan.FromMilliseconds(milliseconds), format, cancellationToken)
                    .ConfigureAwait(false);
                
                return new Command(string.Empty, bytes);
            }));
        }

        #endregion
    }
}
