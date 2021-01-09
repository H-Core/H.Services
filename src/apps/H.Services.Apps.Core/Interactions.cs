using System;
using System.Reactive;
using ReactiveUI;

namespace Dedoose
{
    public static class Interactions
    {
        public static Interaction<Exception, Unit> UserError { get; } = new();
    }
}
