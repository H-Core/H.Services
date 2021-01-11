using System;
using System.Reactive.Linq;
using Dedoose;
using Splat;

#nullable enable

namespace H.Services.Apps.ViewModels
{
    public static class ObservableExtensions
    {
        public static void DefaultCatch(this IObservable<Exception> observable, ViewModelBase viewModelBase)
        {
            observable = observable ?? throw new ArgumentNullException(nameof(observable));
            viewModelBase = viewModelBase ?? throw new ArgumentNullException(nameof(viewModelBase));

            observable.Subscribe(async exception =>
            {
                viewModelBase.Log().Warn(exception);

                await Interactions.UserError.Handle(exception);
            });
        }
    }
}
