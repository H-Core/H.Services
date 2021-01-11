using System;
using System.Reactive;
using System.Reactive.Concurrency;
using H.Core;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace H.Services.Apps.ViewModels
{
    public class MainViewModel : ViewModelBase, IScreen
    {
        #region Properties

        public RoutingState Router { get; } = new();

        private RunnerService RunnerService { get; }

        [Reactive]
        public string Input { get; set; } = string.Empty;

        [Reactive]
        public string Output { get; set; } = string.Empty;

        #region Commands

        public ReactiveCommand<Unit, Unit> Enter { get; }

        #endregion

        #endregion

        #region Constructors

        public MainViewModel(RunnerService runnerService)
        {
            RunnerService = runnerService ?? throw new ArgumentNullException(nameof(runnerService));

            var inputIsNotEmpty = this.WhenAnyValue(
                static viewModel => viewModel.Input,
                static input => !string.IsNullOrWhiteSpace(input));
            Enter = ReactiveCommand.CreateFromTask(async cancellationToken =>
            {
                var input = Input;
                Input = string.Empty;

                await RunnerService.RunAsync(Command.Parse(input), cancellationToken).ConfigureAwait(false);
            }, inputIsNotEmpty);
            Enter.ThrownExceptions.DefaultCatch(this);
        }

        #endregion

        #region Methods

        public void Write(string text)
        {
            text = text ?? throw new ArgumentNullException(nameof(text));

            RxApp.MainThreadScheduler.Schedule(() => Output += text); 
        }

        public void WriteLine(string text)
        {
            text = text ?? throw new ArgumentNullException(nameof(text));

            Write($"{text}{Environment.NewLine}");
        }

        #endregion
    }
}
