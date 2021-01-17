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
        /// <param name="viewModel"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public static void DefaultCatch(
            this IObservable<Exception> observable, 
            ViewModelBase viewModel)
        {
            observable = observable ?? throw new ArgumentNullException(nameof(observable));
            viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));

            observable.Subscribe(async exception =>
            {
                viewModel.Log().Warn(exception);

                await Interactions.UserError.Handle(exception);
            });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="command"></param>
        /// <param name="viewModel"></param>
        /// <exception cref="ArgumentNullException"></exception>
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
