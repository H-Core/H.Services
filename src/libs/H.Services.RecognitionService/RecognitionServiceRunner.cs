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

            Add(AsyncAction.WithoutArguments("start-recognition", service.StartAsync));
            Add(AsyncAction.WithoutArguments("stop-recognition", service.StopAsync));
            
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
            Add(AsyncAction.WithCommand("convert-audio-to-text", async (command, cancellationToken) =>
            {
                var text = await service.ConvertAsync(command.Value.Data, cancellationToken)
                    .ConfigureAwait(false);

                return new Value(text);
            }));
            
            Add(new ProcessAction("send-telegram-voice-message", async (process, command, cancellationToken) =>
            {
                var to = command.Value.Arguments.ElementAtOrDefault(0);

                using var recognition = await service.StartConvertAsync(cancellationToken)
                    .ConfigureAwait(false);
                using var recordingMp3 = await service.StartRecordAsync(RecordingFormat.Mp3, cancellationToken)
                    .ConfigureAwait(false);

                await process.WaitAsync(cancellationToken).ConfigureAwait(false);

                await recordingMp3.StopAsync(cancellationToken).ConfigureAwait(false);
                var result = await recognition.StopAsync(cancellationToken).ConfigureAwait(false);

                var value = new Value(to ?? string.Empty, result)
                {
                    Data = recordingMp3.Data,
                };
                await RunAsync(new Command("telegram voice", value), cancellationToken).ConfigureAwait(false);

                return value;
            }, "to?"));
        }

        #endregion
    }
}
