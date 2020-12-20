using H.Core;

namespace H.Services.Core
{
    /// <summary>
    /// 
    /// </summary>
    public interface IProcessCommandProducer : IServiceBase
    {
        /// <summary>
        /// 
        /// </summary>
        event AsyncEventHandler<ICommand, IProcess<IValue>>? ProcessCommandReceived;
    }
}
