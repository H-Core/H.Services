using System;
using System.Threading;
using System.Threading.Tasks;
using H.Core;
using H.Pipes;
using H.Pipes.Args;
using H.Services.Core;

namespace H.Services
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class IpcClientService : ServiceBase, ICommandProducer
    {
        #region Properties

        private PipeClient<string> PipeClient { get; }

        /// <summary>
        /// 
        /// </summary>
        public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(1);

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
        public event EventHandler<string>? Connected;

        /// <summary>
        /// Invoked whenever a client disconnects from the server.
        /// </summary>
        public event EventHandler<string>? Disconnected;

        private void OnCommandReceived(ICommand value)
        {
            CommandReceived?.Invoke(this, value);
        }

        private void OnConnected(string value)
        {
            Connected?.Invoke(this, value);
        }

        private void OnDisconnected(string value)
        {
            Disconnected?.Invoke(this, value);
        }

        #endregion

        #region Constructors

        /// <summary>
        /// 
        /// </summary>
        public IpcClientService(string pipeName)
        {
            pipeName = pipeName ?? throw new ArgumentNullException(nameof(pipeName));

            PipeClient = new PipeClient<string>(pipeName);
            PipeClient.MessageReceived += PipeServer_OnMessageReceived;
            PipeClient.ExceptionOccurred += (_, args) => OnExceptionOccurred(args.Exception);
            PipeClient.Connected += PipeServer_OnConnected;
            PipeClient.Disconnected += PipeServer_OnDisconnected;
            
            AsyncDisposables.Add(PipeClient);
        }

        #endregion

        #region Event Handlers

        private void PipeServer_OnConnected(object? _, ConnectionEventArgs<string> args)
        {
            try
            {
                OnConnected(args.Connection.Name);

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

        private void PipeServer_OnDisconnected(object? _, ConnectionEventArgs<string> args)
        {
            try
            {
                OnDisconnected(args.Connection.Name);

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
        /// Writes command to server with timeout from property <see cref="Timeout"/>.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task WriteAsync(string value, CancellationToken cancellationToken = default)
        {
            value = value ?? throw new ArgumentNullException(nameof(value));

            using var source = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            source.CancelAfter(Timeout);

            await PipeClient.WriteAsync(value, source.Token).ConfigureAwait(false);
        }

        #endregion
    }
}
