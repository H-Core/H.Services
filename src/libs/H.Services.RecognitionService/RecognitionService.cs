﻿using System;
using System.Collections.Concurrent;
using System.Linq;
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
    public sealed class RecognitionService : ConsumerService, ICommandProducer
    {
        #region Properties

        private ConcurrentDictionary<IStreamingRecognition, bool> Recognitions { get; } = new ();
        
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
        public event AsyncEventHandler<ICommand, IValue>? AsyncCommandReceived;

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

        /// <param name="moduleServices"></param>
        public RecognitionService(params IModuleService[] moduleServices) : base(moduleServices)
        {
        }

        #endregion

        #region Methods

        /// <summary>
        /// 
        /// </summary>
        /// <param name="format"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<IRecording> StartRecordAsync(AudioFormat format, CancellationToken cancellationToken = default)
        {
            if (format is AudioFormat.None)
            {
                throw new ArgumentException($"Invalid format: {format}");
            }
            
            if (InitializeState is not State.Completed)
            {
                await InitializeAsync(cancellationToken).ConfigureAwait(false);
            }
            
            return await Recorder.StartAsync(format, cancellationToken)
                .ConfigureAwait(false);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<string> ConvertAsync(byte[] bytes, CancellationToken cancellationToken = default)
        {
            bytes = bytes ?? throw new ArgumentNullException(nameof(bytes));

            if (InitializeState is not State.Completed)
            {
                await InitializeAsync(cancellationToken).ConfigureAwait(false);
            }

            return await Recognizer.ConvertOverStreamingRecognition(bytes, cancellationToken)
                .ConfigureAwait(false);
        }

        /// <summary>
        /// You must call <see cref="IDisposable.Dispose"/> for recognition.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<IStreamingRecognition> StartConvertAsync(CancellationToken cancellationToken = default)
        {
            if (InitializeState is not State.Completed)
            {
                await InitializeAsync(cancellationToken).ConfigureAwait(false);
            }

            using var exceptions = new ExceptionsBag();
            exceptions.ExceptionOccurred += (_, value) => OnExceptionOccurred(value);

            var recognition = await Recognizer.StartStreamingRecognitionAsync(Recorder, exceptions, cancellationToken)
                .ConfigureAwait(false);
            recognition.Stopped += (_, _) =>
            {
                Recognitions.TryRemove(recognition, out _);

                recognition.Dispose();
            };

            Recognitions.TryAdd(recognition, true);

            return recognition;
        }

        /// <summary>
        /// You must call <see cref="IDisposable.Dispose"/> for recognition.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<IStreamingRecognition> StartAsync(CancellationToken cancellationToken = default)
        {
            if (InitializeState is not State.Completed)
            {
                await InitializeAsync(cancellationToken).ConfigureAwait(false);
            }
            
            using var exceptions = new ExceptionsBag();
            exceptions.ExceptionOccurred += (_, value) => OnExceptionOccurred(value);
            
            var recognition = await Recognizer.StartStreamingRecognitionAsync(Recorder, exceptions, cancellationToken)
                .ConfigureAwait(false);
            recognition.PreviewReceived += (_, value) => OnPreviewCommandReceived(Command.Parse(value));
            recognition.Stopped += (_, value) =>
            {
                OnCommandReceived(Command.Parse(value));
                Recognitions.TryRemove(recognition, out var _);
                
                recognition.Dispose();
            };
                
            Recognitions.TryAdd(recognition, true);
            
            return recognition;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task StopAsync(CancellationToken cancellationToken = default)
        {
            await Task.WhenAll(Recognitions
                    .Keys
                    .Select(async recognition => await recognition.StopAsync(cancellationToken).ConfigureAwait(false)))
                .ConfigureAwait(false);
        }

        #endregion
    }
}
