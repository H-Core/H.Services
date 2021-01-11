using System;
using System.Diagnostics;
using System.Windows;
using H.Services.Apps.Initialization;
using H.Services.Apps.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ReactiveUI;
using Splat;
using HostBuilder = H.Services.Apps.Initialization.HostBuilder;

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
        
        private async void Application_Startup(object _, StartupEventArgs e)
        {
            try
            {
                var view = (Window)GetView<MainViewModel>(out var viewModel);
                view.Show();

                Host.InitializeViewModelsRunners();

                await Host.InitializeServicesAsync(viewModel.WriteLine).ConfigureAwait(false);

                Host.InitializeServiceRunners();
            }
            catch (Exception exception)
            {
                LogHost.Default.Fatal(exception);
            }
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {

        }

        #endregion
    }
}
