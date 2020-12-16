using System;
using System.Threading;
using System.Threading.Tasks;

namespace H.Services.Core
{
    /// <summary>
    /// 
    /// </summary>
    public interface IServiceBase : IAsyncDisposable
    {
        #region Properties

        /// <summary>
        /// 
        /// </summary>
        public State InitializeState { get; set; }
        
        /// <summary>
        /// 
        /// </summary>
        public State DisposeState { get; set; }

        #endregion

        #region Events

        /// <summary>
        /// 
        /// </summary>
        event EventHandler<Exception>? ExceptionOccurred;

        #endregion

        #region Methods

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task InitializeAsync(CancellationToken cancellationToken = default);

        #endregion
    }
}
