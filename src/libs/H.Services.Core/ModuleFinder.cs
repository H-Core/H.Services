using System;
using System.Collections.Generic;
using System.Linq;
using H.Core;
using H.Core.Recognizers;
using H.Core.Recorders;
using H.Core.Runners;

namespace H.Services.Core
{
    /// <summary>
    /// 
    /// </summary>
    public class ModuleFinder : ServiceBase
    {
        #region Properties

        private ICollection<IModuleService> ModuleServices { get; }
        
        /// <summary>
        /// 
        /// </summary>
        public IRecorder Recorder => (IRecorder)ModuleServices
            .SelectMany(service => service.Modules)
            .First(module => module.ShortName.EndsWith("Recorder", StringComparison.OrdinalIgnoreCase));

        /// <summary>
        /// 
        /// </summary>
        public IRecognizer Recognizer => (IRecognizer)ModuleServices
            .SelectMany(service => service.Modules)
            .First(module => module.ShortName.EndsWith("Recognizer", StringComparison.OrdinalIgnoreCase));

        /// <summary>
        /// 
        /// </summary>
        public IEnumerable<IRunner> Runners => ModuleServices
            .SelectMany(service => service.Modules)
            .Where(module => module is IRunner)
            .Cast<IRunner>();

        #endregion

        #region Constructors

        /// <param name="moduleServices"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public ModuleFinder(params IModuleService[] moduleServices)
        {
            ModuleServices = moduleServices ?? throw new ArgumentNullException(nameof(moduleServices));

            foreach (var moduleService in moduleServices)
            {
                Dependencies.Add(moduleService);
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// 
        /// </summary>
        /// <param name="command"></param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <returns></returns>
        public IEnumerable<ICall> GetCalls(ICommand command)
        {
            command = command ?? throw new ArgumentNullException(nameof(command));
            
            return Runners
                .Select(runner => runner.TryPrepareCall(command))
                .Where(call => call != null)
                // ReSharper disable once RedundantEnumerableCastCall
                .Cast<ICall>();
        }

        #endregion
    }
}
