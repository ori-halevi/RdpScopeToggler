using Microsoft.Extensions.DependencyInjection;
using Prism.Ioc;
using Prism.Navigation.Regions;
using RdpScopeToggler.Managers;
using RdpScopeToggler.Services.FilesService;
using RdpScopeToggler.Services.LanguageService;
using RdpScopeToggler.Services.LoggerService;
using RdpScopeToggler.Services.PipeClientService;
using RdpScopeToggler.Services.ServiceExtractor;
using RdpScopeToggler.Services.ServiceInstallationManager;
using RdpScopeToggler.Services.SettingsService;
using RdpScopeToggler.Services.UpdateCheckerService;
using RdpScopeToggler.Services.WindowsServiceManager;
using RdpScopeToggler.ViewModels;
using RdpScopeToggler.Views;
using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Reflection;
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
        private CancellationTokenSource _cts;

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
            containerRegistry.RegisterSingleton<IUpdateCheckerService, UpdateCheckerService>();
            // Unity + IHttpClientFactory
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddHttpClient();
            var serviceProvider = serviceCollection.BuildServiceProvider();

            containerRegistry.RegisterInstance(serviceProvider.GetRequiredService<IHttpClientFactory>());

            containerRegistry.RegisterSingleton<ISettingsService, SettingsService>();
            containerRegistry.RegisterSingleton<IServiceInstallationManager, ServiceInstallationManager>();
            containerRegistry.RegisterSingleton<IWindowsServiceManager, WindowsServiceManager>();
            containerRegistry.RegisterSingleton<IServiceExtractor, ServiceExtractor>();
            containerRegistry.RegisterSingleton<ILanguageService, LanguageService>();
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

            var updateChecker = Container.Resolve<IUpdateCheckerService>();
            await updateChecker.CheckForUpdatesAsync();

            #region Initialize language

            ILanguageService languageService = Container.Resolve<ILanguageService>();
            languageService.LoadLanguage();

            #endregion

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

            var regionManager = Container.Resolve<IRegionManager>();
            regionManager.RequestNavigate("ContentRegion", "WaitingForServiceUserControl");

            var serviceInstaller = Container.Resolve<IServiceInstallationManager>();

            bool isDebug = false;
            if (!isDebug)
            {
                try
                {
                    await serviceInstaller.InitializeServiceAsync();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        ex.Message,
                        "Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error
                    );
                }
            }


            await Task.Delay(1000);

            var pipeClientService = Container.Resolve<IPipeClientService>();
            Container.Resolve<NavigationManager>();
            _cts = pipeClientService.Cts;

            if (await pipeClientService.ConnectAsync(_cts.Token))
            {
                regionManager.RequestNavigate("ContentRegion", "MainUserControl");
                regionManager.RequestNavigate("ActionsRegion", "HomeUserControl");

                await pipeClientService.AskForUpdate();
            }
            else
            {
                // אופציונלי: תראה הודעת שגיאה/תנסה שוב
                Debug.WriteLine("Couldn't connect to the server.");
            }

        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
            _cts.Cancel();
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
