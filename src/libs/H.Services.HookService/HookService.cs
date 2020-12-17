using System;
using System.Collections;
using System.Collections.Generic;
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
    public sealed class HookService : ServiceBase, ICommandProducer, IEnumerable<BoundCommand>
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
        public event EventHandler<string>? CombinationCaught;

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
        
        private void OnCombinationCaught(string value)
        {
            CombinationCaught?.Invoke(this, value);
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

            KeyboardHook.KeyUp += (_, args) =>
            {
                var combination = new KeysCombination(args.Key, args.IsCtrlPressed, args.IsShiftPressed, args.IsAltPressed);
                var info = new ConsoleKeyInfo(
                    (char)args.Key, (ConsoleKey)args.Key, args.IsShiftPressed, args.IsAltPressed, args.IsCtrlPressed);

                OnCombinationCaught(combination.ToString());

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
