using System;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Input;
using Dedoose;
using ReactiveUI;

namespace H.Services.Apps.Views
{
    public partial class MainPage
    {
        public MainPage()
        {
            InitializeComponent();

            this.WhenActivated(disposable =>
            {
                this.Bind(ViewModel,
                        static viewModel => viewModel.Input,
                        static view => view.InputTextBox.Text)
                    .DisposeWith(disposable);
                this.Bind(ViewModel,
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
                var closeCommand = ReactiveCommand.Create(Close);
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
    }
}
