using System;
using System.ComponentModel;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Input;
using ReactiveUI;

namespace H.Services.Apps.Views
{
    public partial class MainPage
    {
        #region Properties

        private bool IsPreparedToClose { get; set; }

        #endregion

        #region Constructors

        public MainPage()
        {
            InitializeComponent();

            this.WhenActivated(disposable =>
            {
                this.Bind(ViewModel,
                        static viewModel => viewModel.Input,
                        static view => view.InputTextBox.Text)
                    .DisposeWith(disposable);
                this.OneWayBind(ViewModel,
                        static viewModel => viewModel.Output,
                        static view => view.OutputTextBox.Text)
                    .DisposeWith(disposable);

                var showHideCommand = ReactiveCommand.Create(() =>
                {
                    if (Visibility == Visibility.Visible)
                    {
                        Hide();
                    }
                    else
                    {
                        Show();
                    }
                });
                var closeCommand = ReactiveCommand.Create(() =>
                {
                    IsPreparedToClose = true;
                    Close();
                });
                Observable
                    .FromEventPattern(TaskbarIcon, nameof(TaskbarIcon.TrayLeftMouseDown))
                    .Select(_ => Unit.Default)
                    .InvokeCommand(showHideCommand)
                    .DisposeWith(disposable);
                Observable
                    .FromEventPattern(ShowHideMenuItem, nameof(ShowHideMenuItem.Click))
                    .Select(_ => Unit.Default)
                    .InvokeCommand(showHideCommand)
                    .DisposeWith(disposable);
                Observable
                    .FromEventPattern(CloseMenuItem, nameof(CloseMenuItem.Click))
                    .Select(_ => Unit.Default)
                    .InvokeCommand(closeCommand)
                    .DisposeWith(disposable);
                Observable
                    .FromEventPattern<CancelEventArgs>(this, nameof(Closing))
                    .Select(args =>
                    {
                        args.EventArgs.Cancel = !IsPreparedToClose;

                        return Unit.Default;
                    })
                    .Where(_ => !IsPreparedToClose)
                    .InvokeCommand(showHideCommand)
                    .DisposeWith(disposable);

                InputTextBox
                    .Events().KeyUp
                    .Select(static x => x.Key)
                    .Where(static key => key == Key.Enter)
                    .Select(static _ => Unit.Default)
                    .InvokeCommand(ViewModel?.Enter ?? throw new InvalidOperationException("ViewModel is null"))
                    .DisposeWith(disposable);

                Interactions.UserError.RegisterHandler(static context =>
                {
                    MessageBox.Show($"{context.Input}", "Exception:");

                    context.SetOutput(Unit.Default);
                }).DisposeWith(disposable);
            });
        }

        #endregion
    }
}
