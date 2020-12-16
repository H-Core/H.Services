using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace H.Services.Core
{
    /// <summary>
    /// 
    /// </summary>
    public class ServiceBase : IServiceBase
    {
        #region Properties

        /// <summary>
        /// 
        /// </summary>
        public State InitializeState { get; set; } = State.NotStarted;

        /// <summary>
        /// 
        /// </summary>
        public State DisposeState { get; set; } = State.NotStarted;

        /// <summary>
        /// 
        /// </summary>
        protected List<IServiceBase> Dependencies { get; } = new ();

        /// <summary>
        /// 
        /// </summary>
        protected List<IAsyncDisposable> AsyncDisposables { get; } = new();

        /// <summary>
        /// 
        /// </summary>
        protected List<IDisposable> Disposables { get; } = new();

        #endregion

        #region Events

        /// <summary>
        /// 
        /// </summary>
        public event EventHandler<Exception>? ExceptionOccurred;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        protected void OnExceptionOccurred(Exception value)
        {
            ExceptionOccurred?.Invoke(this, value);
        }

        #endregion

        #region Methods

        /// <summary>
        /// 
        /// </summary>
        /// <param name="func"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        protected async Task InitializeAsync(Func<Task>? func, CancellationToken cancellationToken = default)
        {
            switch (InitializeState)
            {
                case State.Completed:
                    return;
                
                case State.InProgress:
                {
                    while (InitializeState == State.InProgress)
                    {
                        await Task.Delay(TimeSpan.FromMilliseconds(1), cancellationToken).ConfigureAwait(false);
                    }
                    if (InitializeState == State.Completed)
                    {
                        return;
                    }

                    break;
                }
            }

            InitializeState = State.InProgress;

            await Task
                .WhenAll(Dependencies
                    .Select(dependency => dependency.InitializeAsync(cancellationToken)))
                .ConfigureAwait(false);

            if (func != null)
            {
                await func().ConfigureAwait(false);
            }
            
            InitializeState = State.Completed;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public virtual async Task InitializeAsync(CancellationToken cancellationToken = default)
        {
            await InitializeAsync(null, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public virtual async ValueTask DisposeAsync()
        {
            switch (DisposeState)
            {
                case State.Completed:
                    return;
                
                case State.InProgress:
                {
                    while (DisposeState == State.InProgress)
                    {
                        await Task.Delay(TimeSpan.FromMilliseconds(1)).ConfigureAwait(false);
                    }
                    if (DisposeState == State.Completed)
                    {
                        return;
                    }

                    break;
                }
            }

            DisposeState = State.InProgress;

            await Task
                .WhenAll(Dependencies
                    .Select(dependency => dependency.DisposeAsync().AsTask()))
                .ConfigureAwait(false);

            foreach (var disposable in Disposables)
            {
                disposable.Dispose();
            }

            foreach (var disposable in AsyncDisposables)
            {
                await disposable.DisposeAsync().ConfigureAwait(false);
            }

            GC.SuppressFinalize(this);

            DisposeState = State.Completed;
        }
        
        #endregion
    }
}
