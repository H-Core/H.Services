using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using H.Core;
using H.Hooks;
using H.Services.Core;

namespace H.Services
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class HookService : ServiceBase, ICommandProducer, IProcessCommandProducer, IEnumerable<BoundCommand>
    {
        #region Properties

        private LowLevelMouseHook MouseHook { get; } = new ();
        private LowLevelKeyboardHook KeyboardHook { get; } = new ();

        /// <summary>
        /// 
        /// </summary>
        private Dictionary<ConsoleKeyInfo, BoundCommand> BoundCommands { get; } = new ();

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
        public event AsyncEventHandler<ICommand, IProcess<IValue>>? ProcessCommandReceived;

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

        private Task<IProcess<IValue>[]> OnProcessCommandReceivedAsync(ICommand value, CancellationToken cancellationToken = default)
        {
            return ProcessCommandReceived.InvokeAsync(this, value, cancellationToken);
        }

        #endregion

        #region Constructors

        /// <summary>
        /// 
        /// </summary>
        public HookService()
        {
            Disposables.Add(MouseHook);
            Disposables.Add(KeyboardHook);

            KeyboardHook.KeyDown += (_, args) =>
            {
                var combination = new KeysCombination(args.Key, args.IsCtrlPressed, args.IsShiftPressed, args.IsAltPressed);
                var info = new ConsoleKeyInfo(
                    (char)args.Key, (ConsoleKey)args.Key, args.IsShiftPressed, args.IsAltPressed, args.IsCtrlPressed);

                OnDownCombinationCaught(combination.ToString());

                if (BoundCommands.TryGetValue(info, out var command))
                {
                    OnCommandReceived(command.Command);
                }
            };
            KeyboardHook.KeyUp += (_, args) =>
            {
                var combination = new KeysCombination(args.Key, args.IsCtrlPressed, args.IsShiftPressed, args.IsAltPressed);
                var info = new ConsoleKeyInfo(
                    (char)args.Key, (ConsoleKey)args.Key, args.IsShiftPressed, args.IsAltPressed, args.IsCtrlPressed);

                OnUpCombinationCaught(combination.ToString());

                if (BoundCommands.TryGetValue(info, out var command))
                {
                    OnCommandReceived(command.Command);
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
        public override Task InitializeAsync(CancellationToken cancellationToken = default)
        {
            return InitializeAsync(() =>
            {
                KeyboardHook.Start();
                MouseHook.Start();
            }, cancellationToken);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="command"></param>
        public void Add(BoundCommand command)
        {
            command = command ?? throw new ArgumentNullException(nameof(command));
            
            BoundCommands.Add(command.ConsoleKeyInfo, command);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="command"></param>
        public void While(BoundCommand command)
        {
            var process = (IProcess<IValue>?)null;
            KeyboardHook.KeyDown += async (_, args) =>
            {
                var info = new ConsoleKeyInfo(
                    (char) args.Key, (ConsoleKey) args.Key, args.IsShiftPressed, args.IsAltPressed, args.IsCtrlPressed);

                if (info == command.ConsoleKeyInfo &&
                    process == null)
                {
                    var processes = await OnProcessCommandReceivedAsync(command.Command)
                        .ConfigureAwait(false);
                    process = processes.First();
                }
            };
            KeyboardHook.KeyUp += async (_, args) =>
            {
                var info = new ConsoleKeyInfo(
                    (char)args.Key, (ConsoleKey)args.Key, args.IsShiftPressed, args.IsAltPressed, args.IsCtrlPressed);

                if (info == command.ConsoleKeyInfo &&
                    process != null)
                {
                    await process.StopAsync().ConfigureAwait(false);
                    process = null;
                }
            };
        }
        
        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="combination"></param>
        ///// <returns></returns>
        //public void RunCombination(KeysCombination combination)
        //{
        //    if (!Combinations.TryGetValue(combination, out var command))
        //    {
        //        return;
        //    }

        //    OnCommandReceived(command);
        //}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<KeysCombination?> CatchCombinationAsync(CancellationToken cancellationToken = default)
        {
            var keyboardHookState = KeyboardHook.IsStarted;
            var mouseHookState = MouseHook.IsStarted;

            // Starts if not started
            KeyboardHook.Start();
            MouseHook.Start();

            KeysCombination? combination = null;
            var isCancel = false;

            void OnKeyboardHookOnKeyDown(object? sender, KeyboardHookEventArgs args)
            {
                args.Handled = true;
                if (args.Key == Keys.Escape)
                {
                    isCancel = true;
                    return;
                }

                combination = new KeysCombination(args.Key, args.IsCtrlPressed, args.IsShiftPressed, args.IsAltPressed);
            }

            void OnMouseHookOnMouseDown(object? sender, MouseEventExtArgs args)
            {
                if (args.SpecialButton == 0)
                {
                    return;
                }

                args.Handled = true;
                combination = KeysCombination.FromSpecialData(args.SpecialButton);
            }

            KeyboardHook.KeyDown += OnKeyboardHookOnKeyDown;
            MouseHook.MouseDown += OnMouseHookOnMouseDown;

            try
            {
                while (!isCancel && (combination == null || combination.IsEmpty))
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(1), cancellationToken).ConfigureAwait(false);
                }
            }
            finally
            {
                KeyboardHook.KeyDown -= OnKeyboardHookOnKeyDown;
                MouseHook.MouseDown -= OnMouseHookOnMouseDown;

                KeyboardHook.SetEnabled(keyboardHookState);
                MouseHook.SetEnabled(mouseHookState);
            }

            return isCancel ? null : combination;
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
