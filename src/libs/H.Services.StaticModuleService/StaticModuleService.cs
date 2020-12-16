using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using H.Core;
using H.Services.Core;

namespace H.Services
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class StaticModuleService : ServiceBase, IModuleService
    {
        #region Properties

        /// <summary>
        /// 
        /// </summary>
        public IList<IModule> Modules { get; } = new List<IModule>();

        #endregion

        #region Events

        /// <summary>
        /// 
        /// </summary>
        public event EventHandler<ICommand>? CommandReceived;
        
        /// <summary>
        /// 
        /// </summary>
        public event AsyncEventHandler<ICommand>? AsyncCommandReceived;

        private void OnCommandReceived(ICommand value)
        {
            CommandReceived?.Invoke(this, value);
        }

        private async Task OnAsyncCommandReceivedAsync(ICommand value, CancellationToken cancellationToken = default)
        {
            await AsyncCommandReceived.InvokeAsync(this, value, cancellationToken).ConfigureAwait(false);
        }

        #endregion

        #region Constructors

        /// <summary>
        /// 
        /// </summary>
        /// <param name="modules"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public StaticModuleService(params IModule[] modules)
        {
            modules = modules ?? throw new ArgumentNullException(nameof(modules));

            foreach (var module in modules)
            {
                Add(module);
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// 
        /// </summary>
        /// <param name="module"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public void Add(IModule module)
        {
            module = module ?? throw new ArgumentNullException(nameof(module));

            module.ExceptionOccurred += (_, value) => OnExceptionOccurred(value);
            module.CommandReceived += (_, value) => OnCommandReceived(value);
            module.AsyncCommandReceived += 
                (_, value, token) => OnAsyncCommandReceivedAsync(value, token);

            Modules.Add(module);
            Disposables.Add(module);
        }

        #endregion
    }
}
