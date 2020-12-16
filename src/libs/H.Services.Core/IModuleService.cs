using System.Collections.Generic;
using H.Core;

namespace H.Services.Core
{
    /// <summary>
    /// 
    /// </summary>
    public interface IModuleService : ICommandProducer
    {
        /// <summary>
        /// 
        /// </summary>
        IList<IModule> Modules { get; }
    }
}
