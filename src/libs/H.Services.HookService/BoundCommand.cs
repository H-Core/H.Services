using System;
using System.Collections.Generic;
using H.Core;

namespace H.Services
{
    /// <summary>
    /// 
    /// </summary>
    public class BoundCommand
    {
        #region Properties

        /// <summary>
        /// 
        /// </summary>
        public ICommand Command { get; }

        /// <summary>
        /// 
        /// </summary>
        public bool IsProcessing { get; set; }
        
        /// <summary>
        /// 
        /// </summary>
        public Keys Keys { get; }

        #endregion

        #region Constructors

        /// <summary>
        /// 
        /// </summary>
        /// <param name="command"></param>
        /// <param name="key"></param>
        /// <param name="shift"></param>
        /// <param name="alt"></param>
        /// <param name="ctrl"></param>
        /// <param name="isProcessing"></param>
        public BoundCommand(
            ICommand command, 
            ConsoleKey key, 
            bool shift = false, 
            bool alt = false, 
            bool ctrl = false,
            bool isProcessing = false)
        {
            Command = command ?? throw new ArgumentNullException(nameof(command));

            var keys = new List<Key> { (Key)key };
            if (shift)
            {
                keys.Add(Key.LShift);
            }
            if (alt)
            {
                keys.Add(Key.LAlt);
            }
            if (ctrl)
            {
                keys.Add(Key.LCtrl);
            }
            Keys = new Keys(keys.ToArray());

            IsProcessing = isProcessing;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="command"></param>
        /// <param name="keys"></param>
        /// <param name="isProcessing"></param>
        public BoundCommand(
            ICommand command, 
            Keys keys,
            bool isProcessing = false)
        {
            Command = command ?? throw new ArgumentNullException(nameof(command));
            Keys = keys ?? throw new ArgumentNullException(nameof(keys));
            IsProcessing = isProcessing;
        }

        #endregion
    }
}
