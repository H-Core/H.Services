using System;
using System.Diagnostics;
using System.Windows;
using H.Services.Apps.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ReactiveUI;

#nullable enable

namespace H.Services.Apps
{
    public partial class App
    {
        #region Properties

        public IHost Host { get; }

        #endregion

        #region Constructors

        public App()
        {
            DispatcherUnhandledException += (_, e) =>
            {
                e.Handled = true;
                Trace.WriteLine($"Exception: {e.Exception}");
            };

            Host = HostBuilder
                .Create()
                .AddViews()
                .AddPlatformSpecificLoggers()
                .Build();
        }

        #endregion

        #region Event Handlers

        private IViewFor GetView<T>(out T viewModel) where T : notnull
        {
            viewModel = Host.Services.GetRequiredService<T>();
            var view = Host.Services
                .GetRequiredService<IViewLocator>()
                .ResolveView(viewModel) ??
                throw new InvalidOperationException("View is null.");

            view.ViewModel = viewModel;
            return view;
        }
        
        private void Application_Startup(object _, StartupEventArgs e)
        {
            var view = (Window)GetView<MainViewModel>(out var _);
            view.Show();
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {

        }

        #endregion
    }
}
