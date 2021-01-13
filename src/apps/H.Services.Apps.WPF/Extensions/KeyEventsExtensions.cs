using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Windows.Controls;
using System.Windows.Input;

namespace H.Services.Apps.Extensions
{
    public static class KeyEventsExtensions
    {
        public static IObservable<Unit> WhenKeyUp(this Control control, Key key)
        {
            control = control ?? throw new ArgumentNullException(nameof(control));

            return control
                .Events().KeyUp
                .Where(args => args.Key == key)
                .Select(static _ => Unit.Default);
        }
    }
}
