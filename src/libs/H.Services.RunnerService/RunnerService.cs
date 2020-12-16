using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using H.Core;
using H.Core.Runners;
using H.Services.Core;

namespace H.Services
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class RunnerService : ServiceBase
    {
        #region Properties

        private ModuleFinder ModuleFinder { get; }
        private ConcurrentDictionary<ICall, CancellationTokenSource> CancellationTokenSources { get; } = new();

        /// <summary>
        /// 
        /// </summary>
        public ConcurrentDictionary<ICall, Task> Tasks { get; } = new ();

        #endregion

        #region Events

        /// <summary>
        /// 
        /// </summary>
        public event EventHandler<ICall>? CallRunning;
        
        /// <summary>
        /// 
        /// </summary>
        public event EventHandler<ICall>? CallRan;

        /// <summary>
        /// 
        /// </summary>
        public event EventHandler<ICall>? CallCancelled;

        private void OnCallRunning(ICall value)
        {
            CallRunning?.Invoke(this, value);
        }

        private void OnCallRan(ICall value)
        {
            CallRan?.Invoke(this, value);
        }

        private void OnCallCancelled(ICall value)
        {
            CallCancelled?.Invoke(this, value);
        }

        #endregion

        #region Constructors

        /// <param name="moduleFinder"></param>
        /// <param name="commandProducers"></param>
        public RunnerService(ModuleFinder moduleFinder, params ICommandProducer[] commandProducers)
        {
            ModuleFinder = moduleFinder ?? throw new ArgumentNullException(nameof(moduleFinder));
            commandProducers = commandProducers ?? throw new ArgumentNullException(nameof(commandProducers));

            foreach (var producer in commandProducers)
            {
                producer.CommandReceived += OnCommandReceived;
                producer.AsyncCommandReceived += OnAsyncCommandReceived;

                Dependencies.Add(producer);
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// 
        /// </summary>
        /// <param name="command"></param>
        /// <param name="cancellationToken"></param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <returns></returns>
        public async Task RunAsync(ICommand command, CancellationToken cancellationToken = default)
        {
            command = command ?? throw new ArgumentNullException(nameof(command));
            
            var tasks = new List<Task>();
            foreach (var call in ModuleFinder.GetCalls(command))
            {
                var source = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                CancellationTokenSources.TryAdd(call, source);
                
                var task = Task.Run(async () =>
                {
                    try
                    {
                        OnCallRunning(call);

                        await call.RunAsync(source.Token).ConfigureAwait(false);

                        OnCallRan(call);
                    }
                    catch (OperationCanceledException)
                    {
                        OnCallCancelled(call);
                        
                        throw;
                    }
                    finally
                    {
                        if (CancellationTokenSources.TryRemove(call, out var tokenSource))
                        {
                            tokenSource.Dispose();
                        }
                    }
                }, source.Token);
                
                Tasks.TryAdd(call, task);
                
                tasks.Add(task);
            }
            
            await Task.WhenAll(tasks).ConfigureAwait(false);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="call"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public void CancelCall(ICall call)
        {
            call = call ?? throw new ArgumentNullException(nameof(call));
            
            if (!CancellationTokenSources.TryGetValue(call, out var source))
            {
                return;
            }
            
            source.Cancel();
        }

        /// <summary>
        /// 
        /// </summary>
        public void CancelAll()
        {
            foreach (var call in Tasks.Keys)
            {
                CancelCall(call);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override async ValueTask DisposeAsync()
        {
            CancelAll();

            try
            {
                await Task.WhenAll(Tasks.Values).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
            }
            
            Tasks.Clear();

            foreach (var source in CancellationTokenSources.Values)
            {
                source.Dispose();
            }
            CancellationTokenSources.Clear();
            
            await base.DisposeAsync().ConfigureAwait(false);
        }

        #endregion

        #region Event Handlers

        private async void OnCommandReceived(object _, ICommand value)
        {
            try
            {
                await RunAsync(value).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception exception)
            {
                OnExceptionOccurred(exception);
            }
        }

        private async Task OnAsyncCommandReceived(object _, ICommand value, CancellationToken cancellationToken)
        {
            try
            {
                await RunAsync(value, cancellationToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception exception)
            {
                OnExceptionOccurred(exception);
            }
        }

        #endregion
    }
}
