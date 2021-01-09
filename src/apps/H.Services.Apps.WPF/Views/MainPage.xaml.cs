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
