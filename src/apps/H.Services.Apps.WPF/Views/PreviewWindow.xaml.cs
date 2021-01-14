using System.Reactive.Disposables;
using System.Windows;
using ReactiveUI;

namespace H.Services.Apps.Views
{
    /// <summary>
    /// 
    /// </summary>
    public partial class PreviewWindow
    {
        /// <summary>
        /// 
        /// </summary>
        public PreviewWindow()
        {
            InitializeComponent();

            //var primaryScreen = Screen.PrimaryScreen;
            //var test = SystemParameters.PrimaryScreenWidth;
            //var test2 = SystemParameters.VirtualScreenHeight;
            //Width = primaryScreen.WorkingArea.Width;
            //Height = primaryScreen.WorkingArea.Height;
            //Left = primaryScreen.WorkingArea.Left;
            //Top = primaryScreen.WorkingArea.Top;

            this.WhenActivated(disposable =>
            {
                this.OneWayBind(ViewModel,
                        static viewModel => viewModel.Text,
                        static view => view.TextBlock.Text)
                    .DisposeWith(disposable);

                var showHideCommand = ReactiveCommand.Create<bool>(value =>
                {
                    TextBlock.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
                });
                this.WhenAnyValue(
                        static x => x.TextBlock.Text,
                        static value => !string.IsNullOrWhiteSpace(value))
                    .InvokeCommand(showHideCommand)
                    .DisposeWith(disposable);
            });
        }
    }
}
