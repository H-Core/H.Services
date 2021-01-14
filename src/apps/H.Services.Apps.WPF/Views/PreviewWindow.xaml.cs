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

            Width = SystemParameters.VirtualScreenWidth;
            Height = SystemParameters.VirtualScreenHeight;
            Left = SystemParameters.VirtualScreenLeft;
            Top = SystemParameters.VirtualScreenTop;

            this.WhenActivated(disposable =>
            {
                this.OneWayBind(ViewModel,
                        static viewModel => viewModel.Text,
                        static view => view.TextBlock.Text)
                    .DisposeWith(disposable);
            });
        }
    }
}
