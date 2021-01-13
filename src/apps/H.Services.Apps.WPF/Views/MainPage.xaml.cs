using System;
using System.ComponentModel;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Input;
using H.Services.Apps.Extensions;
using H.Services.Apps.ViewModels;
using ReactiveUI;

namespace H.Services.Apps.Views
{
    public partial class MainPage
    {
        #region Properties

        private bool IsPreparedToClose { get; set; }

        private MainViewModel SafeViewModel => ViewModel ?? 
                                               throw new InvalidOperationException("ViewModel is null.");

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
                    .WhenKeyUp(Key.Enter)
                    .InvokeCommand(SafeViewModel.Enter)
                    .DisposeWith(disposable);

                InputTextBox
                    .WhenKeyUp(Key.Up)
                    .InvokeCommand(SafeViewModel.SetLastInput)
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
