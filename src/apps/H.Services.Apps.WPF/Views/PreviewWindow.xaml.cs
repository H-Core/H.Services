using System.Reactive.Disposables;
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
                if (ViewModel == null)
                {
                    return;
                }

                this.OneWayBind(ViewModel,
                        static viewModel => viewModel.Text,
                        static view => view.TextBlock.Text)
                    .DisposeWith(disposable);
                this.OneWayBind(ViewModel,
                        static viewModel => viewModel.IsActive,
                        static view => view.CommandGrid.Visibility)
                    .DisposeWith(disposable);
                this.BindCommand(ViewModel,
                        static viewModel => viewModel.Close,
                        static view => view.CloseButton)
                    .DisposeWith(disposable);
            });
        }
    }
}
