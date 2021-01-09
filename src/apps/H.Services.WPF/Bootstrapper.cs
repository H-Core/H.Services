using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Caliburn.Micro;
using H.Services;
using HomeCenter.NET.Utilities;
using HomeCenter.NET.ViewModels;
using HomeCenter.NET.ViewModels.Utilities;
using HomeCenter.NET.Views;

namespace HomeCenter
{
    public class Bootstrapper : BootstrapperBase
    {
        private SimpleContainer? Container { get; set; }
        private MainView? MainView { get; set; }

        public Bootstrapper()
        {
            Initialize();
        }

        protected override void Configure()
        {
            Container = new SimpleContainer();

            //Container.Instance(Container);

            Container
                .Singleton<HookService>();

            Container
                .Singleton<IWindowManager, HWindowManager>()
                .Singleton<IEventAggregator, EventAggregator>();

            Container
                .Singleton<PopupViewModel>()
                .Singleton<MainViewModel>();

            base.Configure();
        }

        private static void DisposeObject<T>() where T : class, IDisposable
        {
            var obj = IoC.GetInstance(typeof(T), null) as T ?? throw new ArgumentNullException();

            obj.Dispose();
        }

        private static async ValueTask DisposeAsyncObject<T>() where T : class, IAsyncDisposable
        {
            var obj = IoC.GetInstance(typeof(T), null) as T ?? throw new ArgumentNullException();

            await obj.DisposeAsync().ConfigureAwait(false);
        }

        private static T Get<T>() where T : class 
        {
            return IoC.GetInstance(typeof(T), null) as T ?? throw new ArgumentNullException();
        }

        protected override async void OnStartup(object sender, StartupEventArgs e)
        {
            // Catching unhandled exceptions
            WpfSafeActions.Initialize();

            var manager = Get<IWindowManager>();
            var instance = Get<PopupViewModel>();

            // Create permanent hidden PopupView
            await manager.ShowWindowAsync(instance);
            
            var model = Get<MainViewModel>();
            //var moduleService = Get<ModuleService>();

            var hWindowManager = manager as HWindowManager ?? throw new ArgumentNullException();

            // Create hidden window(without moment of show/hide)
            MainView = await hWindowManager.CreateWindowAsync(model) as MainView ?? throw new ArgumentNullException();

            // TODO: custom window manager is required
            model.IsVisible = false;

            var hookService = Get<HookService>();
            await hookService.InitializeAsync();

            hookService.UpCombinationCaught += (_, value) => model.Print($"Up: {value}");
            hookService.DownCombinationCaught += (_, value) => model.Print($"Down: {value}");
        }

        protected override async void OnExit(object sender, EventArgs e)
        {
            MainView?.Dispose();

            await DisposeAsyncObject<HookService>();
            //DisposeObject<ModuleService>();

            Application.Shutdown();
        }

        protected override object GetInstance(Type service, string key)
        {
            Container = Container ?? throw new InvalidOperationException("Container is null");

            return Container.GetInstance(service, key);
        }

        protected override IEnumerable<object> GetAllInstances(Type service)
        {
            Container = Container ?? throw new InvalidOperationException("Container is null");

            return Container.GetAllInstances(service);
        }

        protected override void BuildUp(object instance)
        {
            Container = Container ?? throw new InvalidOperationException("Container is null");

            Container.BuildUp(instance);
        }

        protected override void OnUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            e.Handled = true;
#if DEBUG
            MessageBox.Show(e.Exception.ToString(), "An error as occurred", MessageBoxButton.OK, MessageBoxImage.Error);
#else           
            MessageBox.Show(e.Exception.Message, "An error as occurred", MessageBoxButton.OK, MessageBoxImage.Error); 
#endif
        }
    }
}
