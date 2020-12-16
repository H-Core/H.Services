using System;
using H.Core;

namespace H.Services.Core
{
    /// <summary>
    /// 
    /// </summary>
    public interface ICommandProducer : IServiceBase
    {
        /// <summary>
        /// 
        /// </summary>
        event AsyncEventHandler<ICommand>? AsyncCommandReceived;
        
        /// <summary>
        /// 
        /// </summary>
        event EventHandler<ICommand>? CommandReceived;
    }
}
