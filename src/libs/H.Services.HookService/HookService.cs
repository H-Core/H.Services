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

        private LowLevelMouseHook MouseHook { get; } = new();
        private LowLevelKeyboardHook KeyboardHook { get; } = new()
        {
            IsLeftRightGranularity = true,
            IsCapsLock = true,
            Handling = true,
            HandleModifierKeys = true,
            IsExtendedMode = true,
        };

        /// <summary>
        /// 
        /// </summary>
        private Dictionary<Keys, BoundCommand> BoundCommands { get; } = new();
        private Dictionary<BoundCommand, IProcess<ICommand>> Processes { get; } = new();

        #endregion

        #region Events

        /// <summary>
        /// 
        /// </summary>
        public event EventHandler<string>? UpCombinationCaught;

        /// <summary>
        /// 
        /// </summary>
        public event EventHandler<string>? DownCombinationCaught;

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

        private void OnCommandReceived(ICommand value)
        {
            CommandReceived?.Invoke(this, value);
        }

        private void OnUpCombinationCaught(string value)
        {
            UpCombinationCaught?.Invoke(this, value);
        }

        private void OnDownCombinationCaught(string value)
        {
            DownCombinationCaught?.Invoke(this, value);
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
            foreach (var boundCommand in boundCommands)
            {
                Add(boundCommand);
            }

            Disposables.Add(MouseHook);
            Disposables.Add(KeyboardHook);

            MouseHook.MouseDown += (_, args) =>
            {
                var combination = Keys.FromSpecialData(args.SpecialButton);

                OnDownCombinationCaught(combination.ToString());
            };
            MouseHook.MouseUp += (_, args) =>
            {
                var combination = Keys.FromSpecialData(args.SpecialButton);

                OnUpCombinationCaught(combination.ToString());
            };
            KeyboardHook.KeyDown += async (_, args) =>
            {
                try
                {
                    var keys = args.ToKeys();
                    OnDownCombinationCaught($"{keys}");

                    if (!BoundCommands.TryGetValue(keys, out var command))
                    {
                        return;
                    }

                    args.IsHandled = true;

                    if (!command.IsProcessing)
                    {
                        OnCommandReceived(command.Command);
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
            };
            KeyboardHook.KeyUp += async (_, args) =>
            {
                try
                {
                    var keys = args.ToKeys();

                    OnUpCombinationCaught($"{keys}");

                    if (!BoundCommands.TryGetValue(keys, out var command))
                    {
                        return;
                    }

                    args.IsHandled = true;

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
            };
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

            void OnKeyboardHookOnKeyDown(object? sender, KeyboardHookEventArgs args)
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

            void OnMouseHookOnMouseDown(object? sender, MouseEventExtArgs args)
            {
                if (args.SpecialButton == 0)
                {
                    return;
                }

                args.Handled = true;
                caughtKeys = Keys.FromSpecialData(args.SpecialButton);
            }

            KeyboardHook.KeyDown += OnKeyboardHookOnKeyDown;
            MouseHook.MouseDown += OnMouseHookOnMouseDown;

            try
            {
                while (!isCancel && (caughtKeys == null || caughtKeys.IsEmpty))
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(1), cancellationToken).ConfigureAwait(false);
                }
            }
            finally
            {
                KeyboardHook.KeyDown -= OnKeyboardHookOnKeyDown;
                MouseHook.MouseDown -= OnMouseHookOnMouseDown;

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
