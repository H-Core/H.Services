using System;
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
    public sealed class HooksService : ServiceBase, ICommandProducer
    {
        #region Properties

        private LowLevelMouseHook MouseHook { get; } = new ();
        private LowLevelKeyboardHook KeyboardHook { get; } = new ();

        /// <summary>
        /// 
        /// </summary>
        public Dictionary<KeysCombination, Command> Combinations { get; } = new ();

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
        public HooksService()
        {
            Disposables.Add(MouseHook);
            Disposables.Add(KeyboardHook);

            KeyboardHook.KeyUp += (_, args) =>
            {
                var combination = new KeysCombination(args.Key, args.IsCtrlPressed, args.IsShiftPressed, args.IsAltPressed);

                OnCombinationCaught(combination.ToString());

                if (Combinations.TryGetValue(combination, out var command))
                {
                    OnCommandReceived(command);
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
        /// <param name="combination"></param>
        /// <returns></returns>
        public void RunCombination(KeysCombination combination)
        {
            if (!Combinations.TryGetValue(combination, out var command))
            {
                return;
            }

            OnCommandReceived(command);
        }

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

        #endregion
    }
}
