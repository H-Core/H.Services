using System;
using System.Reactive.Linq;
using H.Services.Apps.ViewModels;
using Splat;

#nullable enable

namespace H.Services.Apps.Initialization
{
    /// <summary>
    /// 
    /// </summary>
    public static class ObservableExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="observable"></param>
        /// <param name="viewModelBase"></param>
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
