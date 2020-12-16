using System;
using H.Core.Runners;

namespace H.Services
{
    /// <summary>
    /// 
    /// </summary>
    public class IpcClientServiceRunner : Runner
    {
        #region Constructors

        /// <summary>
        /// 
        /// </summary>
        public IpcClientServiceRunner(string actionName, IpcClientService service)
        {
            actionName = actionName ?? throw new ArgumentNullException(nameof(actionName));
            service = service ?? throw new ArgumentNullException(nameof(service));

            Add(new AsyncAction(actionName, service.WriteAsync, "value"));
        }

        #endregion
    }
}
