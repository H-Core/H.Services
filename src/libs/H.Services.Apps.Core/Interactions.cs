using System;
using System.Reactive;
using ReactiveUI;

namespace H.Services.Apps
{
    /// <summary>
    /// 
    /// </summary>
    public static class Interactions
    {
        /// <summary>
        /// 
        /// </summary>
        public static Interaction<Exception, Unit> UserError { get; } = new();
    }
}
