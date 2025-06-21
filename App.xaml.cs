using System.Windows;
using Prism.Navigation;
using Prism.Ioc;
using RdpScopeToggler.Views;
using System.Windows.Navigation;
using Prism.Navigation.Regions;
using RdpScopeToggler.Stores;
using GraphicRdpScopeToggler.Services.FilesService;
using GraphicRdpScopeToggler.Services.RdpService;
using System.Threading.Tasks;
using System.Windows.Threading;
using System;

namespace RdpScopeToggler
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        protected override Window CreateShell()
        {
            return Container.Resolve<MainWindow>();
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterSingleton<IRdpService, RdpService>();
            containerRegistry.RegisterSingleton<IFilesService, FilesService>();

            containerRegistry.RegisterSingleton<TaskInfoStore>();

            containerRegistry.RegisterForNavigation<HomeUserControl>();
            containerRegistry.RegisterForNavigation<WaitingUserControl>();
            containerRegistry.RegisterForNavigation<TaskUserControl>();
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();

            var regionManager = Container.Resolve<IRegionManager>();
            regionManager.RequestNavigate("ContentRegion", "HomeUserControl");
        }





        protected override void OnStartup(StartupEventArgs e)
        {
            // האזנה לשגיאות גלובליות
            AppDomain.CurrentDomain.UnhandledException += OnAppDomainUnhandledException;
            DispatcherUnhandledException += OnDispatcherUnhandledException;
            TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;

            base.OnStartup(e);
        }

        private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            HandleException(e.Exception, "שגיאה ב־UI Thread");
            e.Handled = true; // מונע מהשגיאה להתרסק
        }

        private void OnAppDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is Exception ex)
            {
                HandleException(ex, "שגיאה ב־AppDomain");
            }
        }

        private void OnUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
        {
            HandleException(e.Exception, "שגיאה ב־Task");
            e.SetObserved();
        }

        private void HandleException(Exception ex, string source)
        {
            // לדוגמה – תוכל להחליף ל־Custom Error Window
            MessageBox.Show($"התרחשה שגיאה ({source}):\n{ex.Message}", "שגיאת מערכת", MessageBoxButton.OK, MessageBoxImage.Error);

            // אפשר לשקול לוג (לוג מקומי/קבצים/שרת)
        }








    }
}
