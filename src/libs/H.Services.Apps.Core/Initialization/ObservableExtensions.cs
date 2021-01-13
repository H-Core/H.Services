using System;
using System.Reactive.Linq;
using H.Services.Apps.ViewModels;
using ReactiveUI;
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="command"></param>
        /// <param name="viewModel"></param>
        public static ReactiveCommand<T1, T2> WithDefaultCatch<T1, T2>(
            this ReactiveCommand<T1, T2> command, 
            ViewModelBase viewModel)
        {
            command = command ?? throw new ArgumentNullException(nameof(command));
            viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));

            command.ThrownExceptions.DefaultCatch(viewModel);

            return command;
        }
    }
}
