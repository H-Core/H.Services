using System;
using System.Reactive;
using System.Reactive.Concurrency;
using H.Core;
using H.Services.Apps.Initialization;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace H.Services.Apps.ViewModels
{
    /// <summary>
    /// 
    /// </summary>
    public class MainViewModel : ViewModelBase, IScreen
    {
        #region Properties

        private RunnerService RunnerService { get; }
        private string LastInput { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        public RoutingState Router { get; } = new();

        /// <summary>
        /// 
        /// </summary>
        [Reactive]
        public string Input { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        [Reactive]
        public string Output { get; set; } = string.Empty;

        #region Commands

        /// <summary>
        /// 
        /// </summary>
        public ReactiveCommand<Unit, Unit> Enter { get; }

        /// <summary>
        /// 
        /// </summary>
        public ReactiveCommand<Unit, Unit> SetLastInput { get; }

        #endregion

        #endregion

        #region Constructors

        /// <summary>
        /// 
        /// </summary>
        /// <param name="runnerService"></param>
        public MainViewModel(RunnerService runnerService)
        {
            RunnerService = runnerService ?? throw new ArgumentNullException(nameof(runnerService));

            var inputIsNotEmpty = this.WhenAnyValue(
                static viewModel => viewModel.Input,
                static input => !string.IsNullOrWhiteSpace(input));
            Enter = ReactiveCommand.CreateFromTask(async cancellationToken =>
            {
                LastInput = Input;
                Input = string.Empty;

                await RunnerService.RunAsync(Command.Parse(LastInput), cancellationToken).ConfigureAwait(false);
            }, inputIsNotEmpty).WithDefaultCatch(this);
            SetLastInput = ReactiveCommand.Create(() =>
            {
                Input = LastInput;
            }).WithDefaultCatch(this);
        }

        #endregion

        #region Methods

        /// <summary>
        /// 
        /// </summary>
        /// <param name="text"></param>
        public void Write(string text)
        {
            text = text ?? throw new ArgumentNullException(nameof(text));

            RxApp.MainThreadScheduler.Schedule(() => Output += text); 
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="text"></param>
        public void WriteLine(string text)
        {
            text = text ?? throw new ArgumentNullException(nameof(text));

            Write($"{text}{Environment.NewLine}");
        }

        #endregion
    }
}
