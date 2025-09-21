using Prism.Ioc;
using Prism.Navigation.Regions;
using RdpScopeCommands;
using RdpScopeToggler.Managers;
using RdpScopeToggler.Services.FilesService;
using RdpScopeToggler.Services.LanguageService;
using RdpScopeToggler.Services.LoggerService;
using RdpScopeToggler.Services.PipeClientService;
using RdpScopeToggler.ViewModels;
using RdpScopeToggler.Views;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace RdpScopeToggler
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        public static System.Windows.Forms.NotifyIcon notifyIcon;
        protected override Window CreateShell()
        {
            return Container.Resolve<MainWindow>();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            #region Exception handling
            // האזנה לשגיאות גלובליות
            AppDomain.CurrentDomain.UnhandledException += OnAppDomainUnhandledException;
            DispatcherUnhandledException += OnDispatcherUnhandledException;
            TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;
            #endregion

            // App will only shut down when Shutdown() is called explicitly,
            // not automatically when windows are closed
            Application.Current.ShutdownMode = ShutdownMode.OnExplicitShutdown;
        }


        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            #region Logger service

            const string pathToLoggerFolder = "C:\\ProgramData\\RdpScopeToggler\\Logs";
            containerRegistry.RegisterSingleton<ILoggerService>(() => new LoggerService(pathToLoggerFolder));

            #endregion

            // Register services
            containerRegistry.RegisterSingleton<ILanguageService, LanguageService>();
            containerRegistry.RegisterSingleton<IRdpController, RdpController>();
            containerRegistry.RegisterSingleton<IFilesService, FilesService>();
            containerRegistry.RegisterSingleton<IPipeClientService, PipeClientService>();
            containerRegistry.RegisterSingleton<IndicatorsUserControlViewModel>();


            // Register navigation
            containerRegistry.RegisterForNavigation<WaitingForServiceUserControl>();
            containerRegistry.RegisterForNavigation<HomeUserControl>();
            containerRegistry.RegisterForNavigation<WaitingUserControl>();
            containerRegistry.RegisterForNavigation<TaskUserControl>();
            containerRegistry.RegisterForNavigation<SettingsUserControl>();
            containerRegistry.RegisterForNavigation<WhiteListUserControl>();
            containerRegistry.RegisterForNavigation<MainUserControl>();
            containerRegistry.RegisterForNavigation<LocalAddressesUserControl>();
        }





        protected override async void OnInitialized()
        {
            base.OnInitialized();

            #region Initialize Tray Icon

            var trayIconManager = Container.Resolve<TrayIconManager>();
            trayIconManager.Initialize(
                iconPath: "Assets/remote-desktop.ico",
                tooltip: "Rdp Scope Toggler",
                onOpenWindow: ShowMainWindow,
                onExit: () =>
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        var confirmWindow = new ExitConfirmationWindow();
                        confirmWindow.ShowDialog();

                        if (confirmWindow.UserConfirmed)
                        {
                            trayIconManager.Dispose();
                            Application.Current.Shutdown();
                        }
                    });
                });

            // מחברים את החלון ל־TrayIconManager
            trayIconManager.AttachWindow(MainWindow);

            #endregion

            #region Initialize language

            const string pathToSettingsFile = "C:\\ProgramData\\RdpScopeToggler\\Settings.json";
            Container.Resolve<ILanguageService>().LoadLanguage(pathToSettingsFile);

            #endregion

            var regionManager = Container.Resolve<IRegionManager>();
            regionManager.RequestNavigate("ContentRegion", "WaitingForServiceUserControl");

            var pipeClientService = Container.Resolve<IPipeClientService>();
            Container.Resolve<NavigationManager>();

            if (await pipeClientService.ConnectAsync(CancellationToken.None))
            {
                regionManager.RequestNavigate("ContentRegion", "MainUserControl");
                regionManager.RequestNavigate("ActionsRegion", "HomeUserControl");

                pipeClientService.AskForUpdate();
            }
            else
            {
                // אופציונלי: תראה הודעת שגיאה/תנסה שוב
            }
        }

        private void ShowMainWindow()
        {
            if (MainWindow == null)
                return;

            MainWindow.Show();
            MainWindow.Activate();
        }


        #region Exception handling

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
            MessageBox.Show($"התרחשה שגיאה ({source}):\r\nהודעת השגיאה:\r\n{ex.Message}", "שגיאת מערכת", MessageBoxButton.OK, MessageBoxImage.Error);

            // אפשר לשקול לוג (לוג מקומי/קבצים/שרת)
        }

        #endregion
    }
}
