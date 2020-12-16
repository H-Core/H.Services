using System;
using System.Threading;
using System.Threading.Tasks;
using H.Core;
using H.Core.Recognizers;
using H.Core.Recorders;
using H.Core.Utilities;
using H.Services.Core;

namespace H.Services
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class RecognitionService : ServiceBase, ICommandProducer
    {
        #region Properties

        private ModuleFinder ModuleFinder { get; }

        private IRecognizer Recognizer => ModuleFinder.Recognizer;
        private IRecorder Recorder => ModuleFinder.Recorder;
        
        private IStreamingRecognition? CurrentRecognition { get; set; }

        #endregion

        #region Events

        /// <summary>
        /// 
        /// </summary>
        public event EventHandler<ICommand>? PreviewCommandReceived;

        /// <summary>
        /// 
        /// </summary>
        public event EventHandler<ICommand>? CommandReceived;

        /// <summary>
        /// 
        /// </summary>
        public event AsyncEventHandler<ICommand>? AsyncCommandReceived;

        private void OnPreviewCommandReceived(ICommand value)
        {
            PreviewCommandReceived?.Invoke(this, value);
        }
        
        private void OnCommandReceived(ICommand value)
        {
            CommandReceived?.Invoke(this, value);
        }

        #endregion

        #region Constructors

        /// <param name="moduleFinder"></param>
        public RecognitionService(ModuleFinder moduleFinder)
        {
            ModuleFinder = moduleFinder ?? throw new ArgumentNullException(nameof(moduleFinder));
            
            Dependencies.Add(ModuleFinder);
        }

        #endregion

        #region Methods

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task StartAsync(CancellationToken cancellationToken = default)
        {
            if (InitializeState is not State.Completed)
            {
                await InitializeAsync(cancellationToken).ConfigureAwait(false);
            }
            
            if (CurrentRecognition != null)
            {
                await StopAsync(cancellationToken).ConfigureAwait(false);
            }
            
            var exceptions = new ExceptionsBag();
            exceptions.ExceptionOccurred += (_, value) => OnExceptionOccurred(value);
            
            // TODO: EXCLUDE WRITE WAV HEADER FROM LOGIC.
            CurrentRecognition = await Recognizer.StartStreamingRecognitionAsync(
                Recorder, true, exceptions, cancellationToken)
                .ConfigureAwait(false);
            CurrentRecognition.PartialResultsReceived += (_, value) => OnPreviewCommandReceived(Command.Parse(value));
            CurrentRecognition.FinalResultsReceived += (_, value) => OnCommandReceived(Command.Parse(value));
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task StopAsync(CancellationToken cancellationToken = default)
        {
            if (CurrentRecognition == null)
            {
                return;
            }
            
            await CurrentRecognition.StopAsync(cancellationToken).ConfigureAwait(false);

            CurrentRecognition.Dispose();
            CurrentRecognition = null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override async ValueTask DisposeAsync()
        {
            await StopAsync().ConfigureAwait(false);

            await base.DisposeAsync().ConfigureAwait(false);
        }

        #endregion
    }
}
