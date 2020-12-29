using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
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
    public sealed class RunnerService : ConsumerService
    {
        #region Properties

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

        /// <param name="moduleServices"></param>
        /// <param name="commandProducers"></param>
        public RunnerService(IModuleService[] moduleServices, params ICommandProducer[] commandProducers) : base(moduleServices)
        {
            commandProducers = commandProducers ?? throw new ArgumentNullException(nameof(commandProducers));

            foreach (var producer in commandProducers)
            {
                producer.CommandReceived += OnCommandReceived;
                producer.AsyncCommandReceived += OnAsyncCommandReceived;
                if (producer is IProcessCommandProducer processCommandProducer)
                {
                    processCommandProducer.ProcessCommandReceived += OnProcessCommandReceived;
                }

                Dependencies.Add(producer);
            }
        }

        /// <param name="moduleService"></param>
        /// <param name="commandProducers"></param>
        public RunnerService(IModuleService moduleService, params ICommandProducer[] commandProducers) : 
            this(new [] {moduleService}, commandProducers)
        {
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
        public IProcess<IValue> Start(ICommand command, CancellationToken cancellationToken = default)
        {
            command = command ?? throw new ArgumentNullException(nameof(command));

            var call = GetCalls(command).First();
            var source = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            var process = new Process<IValue>();
            
            CancellationTokenSources.TryAdd(call, source);

            var task = Task.Run(async () =>
            {
                try
                {
                    OnCallRunning(call);

                    var result = await call.RunAsync(process, source.Token).ConfigureAwait(false);

                    OnCallRan(call);
                    
                    return result;
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

            process.Initialize(async () =>
            {
                var output = await task.ConfigureAwait(false);

                return output.Output;
            }, cancellationToken);
            
            Tasks.TryAdd(call, task);

            return process;
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="command"></param>
        /// <param name="cancellationToken"></param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <returns></returns>
        public async Task<ICommand[]> RunAsync(ICommand command, CancellationToken cancellationToken = default)
        {
            command = command ?? throw new ArgumentNullException(nameof(command));
            
            var tasks = new List<Task<ICommand>>();
            var calls = GetCalls(command).ToArray();
            if (!calls.Any())
            {
                throw new ArgumentException($"Command is not supported: {command}");
            }
            
            foreach (var call in calls)
            {
                var source = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                CancellationTokenSources.TryAdd(call, source);
                
                var task = Task.Run(async () =>
                {
                    try
                    {
                        OnCallRunning(call);
                        
                        var value = await call.RunAsync(source.Token).ConfigureAwait(false);

                        OnCallRan(call);

                        return value;
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
            
            return await Task.WhenAll(tasks).ConfigureAwait(false);
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

        private async Task<IValue> OnAsyncCommandReceived(object _, ICommand command, CancellationToken cancellationToken)
        {
            try
            {
                var values = await RunAsync(command, cancellationToken).ConfigureAwait(false);
                var value = values.First();

                return value.Output;
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception exception)
            {
                OnExceptionOccurred(exception);
            }

            return Value.Empty;
        }

        private Task<IProcess<IValue>> OnProcessCommandReceived(object _, ICommand command, CancellationToken cancellationToken)
        {
            try
            {
                return Task.FromResult(Start(command, cancellationToken));
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception exception)
            {
                OnExceptionOccurred(exception);
            }

            return Task.FromResult<IProcess<IValue>>(new Process<IValue>());
        }

        #endregion
    }
}
