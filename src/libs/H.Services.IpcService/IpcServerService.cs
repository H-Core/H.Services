using System;
using System.Threading;
using System.Threading.Tasks;
using H.Core;
using H.Pipes;
using H.Pipes.AccessControl;
using H.Pipes.Args;
using H.Services.Core;

namespace H.Services
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class IpcServerService : ServiceBase, ICommandProducer
    {
        #region Properties

        private PipeServer<string> PipeServer { get; }

        /// <summary>
        /// 
        /// </summary>
        public Func<ConnectionEventArgs<string>, ICommand>? ConnectedCommandFactory { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public Func<ConnectionEventArgs<string>, ICommand>? DisconnectedCommandFactory { get; set; }

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
        
        /// <summary>
        /// Invoked whenever a client connects to the server.
        /// </summary>
        public event EventHandler<string>? ClientConnected;

        /// <summary>
        /// Invoked whenever a client disconnects from the server.
        /// </summary>
        public event EventHandler<string>? ClientDisconnected;

        private void OnCommandReceived(ICommand value)
        {
            CommandReceived?.Invoke(this, value);
        }

        private void OnClientConnected(string value)
        {
            ClientConnected?.Invoke(this, value);
        }

        private void OnClientDisconnected(string value)
        {
            ClientDisconnected?.Invoke(this, value);
        }

        #endregion

        #region Constructors

        /// <summary>
        /// 
        /// </summary>
        public IpcServerService(string pipeName)
        {
            pipeName = pipeName ?? throw new ArgumentNullException(nameof(pipeName));

            PipeServer = new PipeServer<string>(pipeName);
            PipeServer.MessageReceived += PipeServer_OnMessageReceived;
            PipeServer.ExceptionOccurred += (_, args) => OnExceptionOccurred(args.Exception);
            PipeServer.ClientConnected += PipeServer_OnClientConnected;
            PipeServer.ClientDisconnected += PipeServer_OnClientDisconnected;
            PipeServer.AllowUsersReadWrite();
            
            AsyncDisposables.Add(PipeServer);
        }

        #endregion

        #region Event Handlers

        private void PipeServer_OnClientConnected(object? _, ConnectionEventArgs<string> args)
        {
            try
            {
                OnClientConnected(args.Connection.Name);

                if (ConnectedCommandFactory != null)
                {
                    OnCommandReceived(ConnectedCommandFactory(args));
                }
            }
            catch (Exception exception)
            {
                OnExceptionOccurred(exception);
            }
        }

        private void PipeServer_OnClientDisconnected(object? _, ConnectionEventArgs<string> args)
        {
            try
            {
                OnClientDisconnected(args.Connection.Name);

                if (DisconnectedCommandFactory != null)
                {
                    OnCommandReceived(DisconnectedCommandFactory(args));
                }
            }
            catch (Exception exception)
            {
                OnExceptionOccurred(exception);
            }
        }

        private void PipeServer_OnMessageReceived(object? _, ConnectionMessageEventArgs<string?> args)
        {
            try
            {
                OnCommandReceived(Command.Parse(args.Message ?? string.Empty));
            }
            catch (Exception exception)
            {
                OnExceptionOccurred(exception);
            }
        }

        #endregion

        #region Public methods

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public override Task InitializeAsync(CancellationToken cancellationToken = default)
        {
            return InitializeAsync(async () =>
            {
                await PipeServer.StartAsync(cancellationToken).ConfigureAwait(false);
            }, cancellationToken);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task WriteAsync(string value, CancellationToken cancellationToken = default)
        {
            value = value ?? throw new ArgumentNullException(nameof(value));

            if (InitializeState is not State.Completed)
            {
                await InitializeAsync(cancellationToken).ConfigureAwait(false);
            }
            
            await PipeServer.WriteAsync(value, cancellationToken).ConfigureAwait(false);
        }

        #endregion
    }
}
