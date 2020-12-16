using System;
using H.Core.Runners;

namespace H.Services
{
    /// <summary>
    /// 
    /// </summary>
    public class RecognitionServiceRunner : Runner
    {
        #region Constructors

        /// <summary>
        /// 
        /// </summary>
        public RecognitionServiceRunner(RecognitionService service)
        {
            service = service ?? throw new ArgumentNullException(nameof(service));

            Add(new AsyncAction("start-recognition", service.StartAsync));
            Add(new AsyncAction("stop-recognition", service.StopAsync));
        }

        #endregion
    }
}
