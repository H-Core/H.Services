using System;
using System.Reactive;
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

            Cancel = ReactiveCommand.Create(() => {}).WithDefaultCatch(this);
        }

        #endregion
    }
}
