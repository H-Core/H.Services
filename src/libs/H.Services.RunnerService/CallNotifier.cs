using System;
using H.Core;
using H.Core.Notifiers;

namespace H.Services
{
    /// <summary>
    /// 
    /// </summary>
    public class CallNotifier : Notifier
    {
        #region Constructors

        /// <summary>
        /// 
        /// </summary>
        public CallNotifier(RunnerService service, ICommand command, string name)
        {
            service = service ?? throw new ArgumentNullException(nameof(service));
            Command = command ?? throw new ArgumentNullException(nameof(command));

            service.CallRunning += (_, call) =>
            {
                if (!string.Equals(call.Command.Name, name, StringComparison.Ordinal))
                {
                    return;
                }

                OnEventOccurred();
            };
        }

        #endregion
    }
}
