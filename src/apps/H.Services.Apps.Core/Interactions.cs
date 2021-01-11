using System;
using System.Reactive;
using ReactiveUI;

namespace H.Services.Apps
{
    public static class Interactions
    {
        public static Interaction<Exception, Unit> UserError { get; } = new();
    }
}
