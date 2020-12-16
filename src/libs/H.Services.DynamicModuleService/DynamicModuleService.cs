using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using H.Containers;
using H.Core;
using H.Core.Recognizers;
using H.Core.Recorders;
using H.IO.Utilities;
using H.Modules;
using H.Services.Core;

namespace H.Services
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class DynamicModuleService : ServiceBase, IModuleService
    {
        #region Properties

        private ModuleManager<IModule> ModuleManager { get; } = new (
            Path.Combine(Path.GetTempPath(), "H.Logic"));
        
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
        public DynamicModuleService()
        {
            AsyncDisposables.Add(ModuleManager);
        }

        #endregion

        #region Methods

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public override Task InitializeAsync(CancellationToken cancellationToken = default)
        {
            return InitializeAsync(async () =>
            {
                var recorder = await AddAsync<IRecorder>("H.Recorders.NAudioRecorder", cancellationToken)
                    .ConfigureAwait(false);
                var recognizer = await AddAsync<IRecognizer>("H.Converters.WitAiConverter", cancellationToken)
                    .ConfigureAwait(false);

                recognizer.SetSetting("Token", "XZS4M3BUYV5LBMEWJKAGJ6HCPWZ5IDGY");

                foreach (var module in new [] { recorder, recognizer })
                {
                    module.ExceptionOccurred += (_, value) => OnExceptionOccurred(value);
                    module.CommandReceived += (_, value) => OnCommandReceived(value);
                    module.AsyncCommandReceived +=
                        (_, value, token) => OnAsyncCommandReceivedAsync(value, token);

                    Modules.Add(module);
                }
            }, cancellationToken);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <param name="cancellationToken"></param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <returns></returns>
        public async Task<IModule> AddAsync<T>(string name, CancellationToken cancellationToken = default)
            where T : class, IModule
        {
            name = name ?? throw new ArgumentNullException(nameof(name));
            
#pragma warning disable CA2000 // Dispose objects before losing scope
            var container = new ProcessContainer(name);
#pragma warning restore CA2000 // Dispose objects before losing scope

            var instance = await ModuleManager.AddModuleAsync<T>(
                    container,
                    name,
                    name,
                    ResourcesUtilities.ReadFileAsBytes($"{name}.zip"),
                    cancellationToken)
                .ConfigureAwait(false);

            Modules.Add(instance);

            return instance;
        }
        
        #endregion
    }
}
