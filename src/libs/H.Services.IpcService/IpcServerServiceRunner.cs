using System;
using H.Core.Runners;

namespace H.Services
{
    /// <summary>
    /// 
    /// </summary>
    public class IpcServerServiceRunner : Runner
    {
        #region Constructors

        /// <summary>
        /// 
        /// </summary>
        public IpcServerServiceRunner(string actionName, IpcServerService service)
        {
            actionName = actionName ?? throw new ArgumentNullException(nameof(actionName));
            service = service ?? throw new ArgumentNullException(nameof(service));

            Add(new AsyncAction(actionName, service.WriteAsync, "value"));
        }

        #endregion
    }
}
