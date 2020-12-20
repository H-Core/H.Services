using System;
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
        public ConsoleKeyInfo ConsoleKeyInfo { get; }

        #endregion

        #region Constructors

        /// <summary>
        /// 
        /// </summary>
        /// <param name="command"></param>
        /// <param name="key"></param>
        /// <param name="shift"></param>
        /// <param name="alt"></param>
        /// <param name="control"></param>
        public BoundCommand(ICommand command, ConsoleKey key, bool shift = false, bool alt = false, bool control = false)
        {
            Command = command ?? throw new ArgumentNullException(nameof(command));
            
            ConsoleKeyInfo = new ConsoleKeyInfo((char)key, key, shift, alt, control);
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="command"></param>
        /// <param name="key"></param>
        /// <param name="shift"></param>
        /// <param name="alt"></param>
        /// <param name="control"></param>
        public BoundCommand(ICommand command, char key, bool shift = false, bool alt = false, bool control = false)
        {
            Command = command ?? throw new ArgumentNullException(nameof(command));

            ConsoleKeyInfo = new ConsoleKeyInfo(key, (ConsoleKey)key, shift, alt, control);
        }

        #endregion
    }
}
