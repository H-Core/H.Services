using System;
using System.Linq.Expressions;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dedoose;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Splat;

#nullable enable

namespace H.Services.Apps.ViewModels
{
    public abstract class ViewModelBase : ReactiveObject
    {
        #region Methods

        public static ReactiveCommand<Unit, TReturn> CreateCommand<TReturn, TViewModel>(
            TViewModel viewModel,
            Func<Task<TReturn>> task,
            Expression<Func<TViewModel, TReturn>> expression)
            where TViewModel : ViewModelBase
        {
            var command = ReactiveCommand.CreateFromTask(task);
            command.ToPropertyEx(viewModel, expression);
            command.ThrownExceptions.Subscribe(exception =>
            {
                viewModel.Log().Warn(exception);
            });
            command.Execute().Subscribe();

            return command;
        }

        public static ReactiveCommand<Unit, Unit> CreateCommand<TViewModel>(
            TViewModel viewModel,
            Func<CancellationToken, Task> task,
            IObservable<bool>? canExecute = null)
            where TViewModel : ViewModelBase
        {
            var command = ReactiveCommand.CreateFromTask(task, canExecute);
            command.ThrownExceptions.Subscribe(async exception =>
            {
                viewModel.Log().Warn(exception);

                await Interactions.UserError.Handle(exception);
            });

            return command;
        }

        #endregion
    }
}
