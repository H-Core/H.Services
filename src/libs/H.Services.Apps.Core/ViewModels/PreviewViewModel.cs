using System;
using System.Reactive;
using System.Reactive.Linq;
using H.Core;
using H.Services.Apps.Initialization;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace H.Services.Apps.ViewModels
{
    /// <summary>
    /// 
    /// </summary>
    public class PreviewViewModel : ViewModelBase
    {
        #region Properties

        private RecognitionService RecognitionService { get; }

        /// <summary>
        /// 
        /// </summary>
        [Reactive]
        public bool IsActive { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [Reactive]
        public bool IsStartedAgain { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [Reactive]
        public string Text { get; set; } = string.Empty;

        #region Commands

        /// <summary>
        /// 
        /// </summary>
        public ReactiveCommand<Unit, Unit> Close { get; }

        #endregion

        #endregion

        #region Constructors

        /// <summary>
        /// 
        /// </summary>
        /// <param name="recognitionService"></param>
        public PreviewViewModel(RecognitionService recognitionService)
        {
            RecognitionService = recognitionService ?? throw new ArgumentNullException(nameof(recognitionService));

            Observable.FromEventPattern(
                    RecognitionService,
                    nameof(RecognitionService.Started))
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(_ =>
                {
                    IsStartedAgain = IsActive;
                    Text = "Waiting command...";
                    IsActive = true;
                });
            Observable.FromEventPattern<ICommand>(
                    RecognitionService,
                    nameof(RecognitionService.PreviewCommandReceived))
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(pattern =>
                {
                    Text = $"{pattern.EventArgs}";
                });
            Observable.FromEventPattern<ICommand>(
                    RecognitionService,
                    nameof(RecognitionService.CommandReceived))
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(pattern =>
                {
                    Text = pattern.EventArgs.IsEmpty
                        ? "Didn't get that."
                        : $"{pattern.EventArgs}";
                });
            Observable.FromEventPattern<ICommand>(
                    RecognitionService,
                    nameof(RecognitionService.CommandReceived))
                .Delay(TimeSpan.FromSeconds(3))
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(_ =>
                {
                    IsActive = IsStartedAgain;
                    Text = IsStartedAgain ? Text : string.Empty;
                    IsStartedAgain = false;
                });

            Close = ReactiveCommand.CreateFromTask(async cancellationToken =>
            {
                IsActive = false;

                await RecognitionService.StopAsync(cancellationToken).ConfigureAwait(false);
            }).WithDefaultCatch(this);
        }

        #endregion
    }
}
