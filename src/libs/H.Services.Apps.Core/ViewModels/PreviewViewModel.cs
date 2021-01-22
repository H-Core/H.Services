using System;
using System.Reactive;
using System.Threading.Tasks;
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
        public ReactiveCommand<Unit, Unit> Cancel { get; }

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
            RecognitionService.Started += (_, _) =>
            {
                IsStartedAgain = IsActive;
                Text = "Waiting command...";
                IsActive = true;
            };
            RecognitionService.PreviewCommandReceived += (_, value) =>
            {
                Text = $"{value}";
            };
            RecognitionService.CommandReceived += async (_, value) =>
            {
                Text = value.IsEmpty
                    ? "Didn't get that."
                    : $"{value}";

                await Task.Delay(TimeSpan.FromSeconds(3)).ConfigureAwait(false);

                IsActive = IsStartedAgain;
                Text = IsStartedAgain ? Text : string.Empty;
                IsStartedAgain = false;
            };

            Cancel = ReactiveCommand.Create(() => { }).WithDefaultCatch(this);
        }

        #endregion
    }
}
