using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using H.Core;
using H.Hooks;
using H.Services.Core;
using H.Services.Extensions;
using Keys = H.Core.Keys;
using Key = H.Core.Key;

namespace H.Services
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class HookService : ServiceBase, ICommandProducer, IProcessCommandProducer, IEnumerable<BoundCommand>
    {
        #region Properties

        private LowLevelMouseHook MouseHook { get; } = new()
        {
            Handling = true,
            AddKeyboardKeys = true,
            IsLeftRightGranularity = true,
            IsCapsLock = true,
            IsExtendedMode = true,
        };
        private LowLevelKeyboardHook KeyboardHook { get; } = new()
        {
            Handling = true,
            HandleModifierKeys = true,
            IsLeftRightGranularity = true,
            IsCapsLock = true,
            IsExtendedMode = true,
        };

        private Dictionary<Keys, BoundCommand> BoundCommands { get; } = new();
        private Dictionary<BoundCommand, IProcess<ICommand>> Processes { get; } = new();

        #endregion

        #region Events

        /// <summary>
        /// 
        /// </summary>
        public event EventHandler<Keys>? UpCaught;

        /// <summary>
        /// 
        /// </summary>
        public event EventHandler<Keys>? DownCaught;

        /// <summary>
        /// 
        /// </summary>
        public event EventHandler<BoundCommand>? BoundUpCaught;

        /// <summary>
        /// 
        /// </summary>
        public event EventHandler<BoundCommand>? BoundDownCaught;

        /// <summary>
        /// 
        /// </summary>
        public event EventHandler<ICommand>? CommandReceived;

        /// <summary>
        /// 
        /// </summary>
        public event AsyncEventHandler<ICommand, IValue>? AsyncCommandReceived;

        /// <summary>
        /// 
        /// </summary>
        public event AsyncEventHandler<ICommand, IProcess<ICommand>>? ProcessCommandReceived;

        private void OnUpCaught(Keys value)
        {
            UpCaught?.Invoke(this, value);
        }

        private void OnDownCaught(Keys value)
        {
            DownCaught?.Invoke(this, value);
        }

        private void OnBoundUpCaught(BoundCommand value)
        {
            BoundUpCaught?.Invoke(this, value);
        }

        private void OnBoundDownCaught(BoundCommand value)
        {
            BoundDownCaught?.Invoke(this, value);
        }

        private void OnCommandReceived(ICommand value)
        {
            CommandReceived?.Invoke(this, value);
        }

        private Task<IProcess<ICommand>[]> OnProcessCommandReceivedAsync(ICommand value, CancellationToken cancellationToken = default)
        {
            return ProcessCommandReceived.InvokeAsync(this, value, cancellationToken);
        }

        #endregion

        #region Constructors

        /// <summary>
        /// 
        /// </summary>
        public HookService(params BoundCommand[] boundCommands)
        {
            Disposables.Add(MouseHook);
            Disposables.Add(KeyboardHook);

            MouseHook.ExceptionOccurred += (_, value) => OnExceptionOccurred(value);
            MouseHook.Down += Hook_OnDown;
            MouseHook.Up += Hook_OnUp;
            KeyboardHook.ExceptionOccurred += (_, value) => OnExceptionOccurred(value);
            KeyboardHook.Down += Hook_OnDown;
            KeyboardHook.Up += Hook_OnUp;

            foreach (var boundCommand in boundCommands)
            {
                Add(boundCommand);
            }
        }

        #endregion

        #region Event Handlers

        private async void Hook_OnUp(object _, KeyboardEventArgs args)
        {
            try
            {
                var keys = args.ToKeys();

                OnUpCaught(keys);

                if (!BoundCommands.TryGetValue(keys, out var command))
                {
                    return;
                }

                args.IsHandled = true;

                OnBoundUpCaught(command);

                if (!command.IsProcessing)
                {
                    OnCommandReceived(command.Command);
                    return;
                }

                if (!Processes.TryGetValue(command, out var process))
                {
                    return;
                }

                await process.StopAsync().ConfigureAwait(false);
                Processes.Remove(command);
            }
            catch (Exception exception)
            {
                OnExceptionOccurred(exception);
            }
        }

        private async void Hook_OnDown(object _, KeyboardEventArgs args)
        {
            try
            {
                var keys = args.ToKeys();
                OnDownCaught(keys);

                if (!BoundCommands.TryGetValue(keys, out var command))
                {
                    return;
                }

                args.IsHandled = true;

                OnBoundDownCaught(command);

                if (!command.IsProcessing)
                {
                    return;
                }

                if (Processes.ContainsKey(command))
                {
                    return;
                }

                var processes = await OnProcessCommandReceivedAsync(command.Command)
                    .ConfigureAwait(false);
                var process = processes.First();

                Processes[command] = process;
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
        public override async Task InitializeAsync(CancellationToken cancellationToken = default)
        {
            await InitializeAsync(() =>
            {
                KeyboardHook.Start();
                MouseHook.Start();
            }, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="command"></param>
        public void Add(BoundCommand command)
        {
            command = command ?? throw new ArgumentNullException(nameof(command));

            BoundCommands.Add(command.Keys, command);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<Keys?> CatchCombinationAsync(CancellationToken cancellationToken = default)
        {
            var keyboardHookState = KeyboardHook.IsStarted;
            var mouseHookState = MouseHook.IsStarted;

            // Starts if not started
            KeyboardHook.Start();
            MouseHook.Start();

            Keys? caughtKeys = null;
            var isCancel = false;

            void OnKeyboardHookOnKeyDown(object? _, KeyboardEventArgs args)
            {
                var keys = args.ToKeys();

                args.IsHandled = true;
                if (keys.Values.Contains(Key.Escape))
                {
                    isCancel = true;
                    return;
                }

                caughtKeys = keys;
            }

            void OnMouseHookOnMouseDown(object? _, MouseEventArgs args)
            {
                args.IsHandled = true;
                caughtKeys = args.ToKeys();
            }

            KeyboardHook.Down += OnKeyboardHookOnKeyDown;
            MouseHook.Down += OnMouseHookOnMouseDown;

            try
            {
                while (!isCancel && (caughtKeys == null || caughtKeys.IsEmpty))
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(1), cancellationToken).ConfigureAwait(false);
                }
            }
            finally
            {
                KeyboardHook.Down -= OnKeyboardHookOnKeyDown;
                MouseHook.Down -= OnMouseHookOnMouseDown;

                if (keyboardHookState)
                {
                    KeyboardHook.Start();
                }
                if (mouseHookState)
                {
                    MouseHook.Start();
                }
            }

            return isCancel ? null : caughtKeys;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IEnumerator<BoundCommand> GetEnumerator()
        {
            return BoundCommands.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return BoundCommands.Values.GetEnumerator();
        }

        #endregion
    }
}
