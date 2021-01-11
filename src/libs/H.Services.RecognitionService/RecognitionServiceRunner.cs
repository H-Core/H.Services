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
                var settings = AudioSettings.Parse(command.Input.Argument);
                
                using var recording = await service.StartRecordAsync(settings, cancellationToken)
                    .ConfigureAwait(false);

                await process.WaitAsync(cancellationToken).ConfigureAwait(false);

                await recording.StopAsync(cancellationToken).ConfigureAwait(false);
                
                return new Value(recording.Data);
            }, "Arguments: audioSettings?"));
            Add(AsyncAction.WithCommand("convert-audio-to-text", async (command, cancellationToken) =>
            {
                var settings = AudioSettings.Parse(command.Input.Argument);

                var text = await service.ConvertAsync(command.Input.Data, settings, cancellationToken)
                    .ConfigureAwait(false);

                return new Value(text);
            }, "Arguments: audioSettings?"));
            
            Add(new ProcessAction("send-telegram-voice-message", async (process, command, cancellationToken) =>
            {
                var to = command.Input.Arguments.ElementAtOrDefault(0);

                using var recognition = await service.StartConvertAsync(cancellationToken)
                    .ConfigureAwait(false);
                using var recording = await service.StartRecordAsync(new AudioSettings(AudioFormat.Mp3), cancellationToken)
                    .ConfigureAwait(false);

                await process.WaitAsync(cancellationToken).ConfigureAwait(false);

                var bytes = await recording.StopAsync(cancellationToken).ConfigureAwait(false);
                var result = await recognition.StopAsync(cancellationToken).ConfigureAwait(false);

                var value = new Value(
                    to ?? string.Empty, 
                    string.IsNullOrWhiteSpace(result) ? "unknown" : result)
                {
                    Data = bytes,
                };
                await RunAsync(new Command("telegram audio", value), cancellationToken).ConfigureAwait(false);

                return value;
            }, "Arguments: to?"));
        }

        #endregion
    }
}
