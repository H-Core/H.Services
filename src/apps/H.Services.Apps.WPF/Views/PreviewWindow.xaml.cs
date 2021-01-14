using System.Reactive.Disposables;
using System.Reactive.Linq;
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

                ViewModel
                    .WhenAnyValue(static x => x!.Text)
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .BindTo(this, static view => view.TextBlock.Text);
                ViewModel
                    .WhenAnyValue(static x => x!.IsActive)
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Select(static value => value ? Visibility.Visible : Visibility.Collapsed)
                    .BindTo(
                        this, 
                        static view => view.TextBlock.Visibility);
            });
        }
    }
}
