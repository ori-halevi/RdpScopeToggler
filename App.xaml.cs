using Microsoft.Extensions.DependencyInjection;
using Prism.Ioc;
using Prism.Navigation.Regions;
using RdpScopeToggler.Helpers;
using RdpScopeToggler.Managers;
using RdpScopeToggler.Models;
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
using System.IO;
using System.Net.Http;
using System.Security.Principal;
using System.Text.Json;
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
        private Mutex _singleInstanceMutex;
        private EventWaitHandle _showWindowEvent;
        private Thread _showWindowListenerThread;
        private volatile bool _showWindowListenerRunning;

        public static System.Windows.Forms.NotifyIcon notifyIcon;
        protected override Window CreateShell()
        {
            return Container.Resolve<MainWindow>();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            var userSid = WindowsIdentity.GetCurrent().User?.Value ?? Environment.UserName;
            var mutexName = $"Local\\RdpScopeToggler_{userSid}";
            var showWindowEventName = $"Local\\RdpScopeToggler_ShowWindow_{userSid}";
            _singleInstanceMutex = new Mutex(initiallyOwned: true, mutexName, out bool createdNew);

            if (!createdNew)
            {
                // Primary instance already running — try to bring its window to the
                // foreground. Only fall back to the "already running" dialog if signalling
                // the primary instance fails.
                if (!TryActivateRunningInstance(showWindowEventName))
                {
                    ShowAlreadyRunningDialog();
                }
                Shutdown();
                return;
            }

            _showWindowEvent = new EventWaitHandle(false, EventResetMode.AutoReset, showWindowEventName);
            _showWindowListenerRunning = true;
            _showWindowListenerThread = new Thread(ShowWindowListenerLoop) { IsBackground = true };
            _showWindowListenerThread.Start();

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

            MainWindow.Closing += (sender, e) =>
            {
                e.Cancel = true;
                ShowCloseOrBackgroundDialog(trayIconManager);
            };

            #endregion

            var regionManager = Container.Resolve<IRegionManager>();
            regionManager.RequestNavigate("ContentRegion", "WaitingForServiceUserControl");

            var serviceInstaller = Container.Resolve<IServiceInstallationManager>();

            // While debugging, don't try to install the service or to run it.
            bool isDebug = false;
            if (!isDebug)
            {
                 await serviceInstaller.InitializeServiceAsync();
            }
            else
            {
                trayIconManager.ShowDebugModeReminder();
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
                throw new Exception("Couldn't connect to the RdpScopeService server.");
            }

        }

        protected override void OnExit(ExitEventArgs e)
        {
            // Shut down the pipe client BEFORE base.OnExit. This clears event subscribers so
            // any in-flight pipe message won't reach UI-bound handlers (NavigationManager,
            // IndicatorsUserControlViewModel, etc.) after Application.Current starts tearing
            // down — which was causing intermittent NullReferenceException on exit.
            try
            {
                var pipe = Container.Resolve<IPipeClientService>();
                pipe.Shutdown();
            }
            catch (Exception ex)
            {
                // Container may already be disposed, or resolution may fail during teardown.
                System.Diagnostics.Debug.WriteLine($"PipeClient shutdown error: {ex.Message}");
            }

            try
            {
                _showWindowListenerRunning = false;
                _showWindowEvent?.Set();
                _showWindowEvent?.Dispose();
            }
            catch { }

            try
            {
                _singleInstanceMutex?.ReleaseMutex();
                _singleInstanceMutex?.Dispose();
            }
            catch { }

            base.OnExit(e);
        }

        private static bool TryActivateRunningInstance(string eventName)
        {
            try
            {
                using var ev = EventWaitHandle.OpenExisting(eventName);
                ev.Set();
                return true;
            }
            catch
            {
                return false;
            }
        }

        private void ShowWindowListenerLoop()
        {
            while (_showWindowListenerRunning)
            {
                try
                {
                    if (!_showWindowEvent.WaitOne())
                        continue;

                    if (!_showWindowListenerRunning)
                        break;

                    Current?.Dispatcher.Invoke(ShowMainWindow);
                }
                catch
                {
                    break;
                }
            }
        }

        private void ShowCloseOrBackgroundDialog(TrayIconManager trayIconManager)
        {
            var dialog = new GenericDialogWindow(new GenericDialogOptions
            {
                Title = TranslationHelper.Translate("CloseAppTitle_translator"),
                Message = TranslationHelper.Translate("CloseOrBackgroundQuestion_translator"),
                Topmost = true,
                Buttons =
                {
                    new DialogButtonConfig
                    {
                        Text = TranslationHelper.Translate("RunInBackground_translator"),
                        IsDefault = true,
                        OnClick = () =>
                        {
                            MainWindow?.Hide();
                            trayIconManager.ShowStillRunningWarning();
                        }
                    },
                    new DialogButtonConfig
                    {
                        Text = TranslationHelper.Translate("CloseCompletely_translator"),
                        StyleKey = "DisconnectButton",
                        OnClick = () =>
                        {
                            trayIconManager.Dispose();
                            Current.Shutdown();
                        }
                    },
                    new DialogButtonConfig
                    {
                        Text = TranslationHelper.Translate("Cancel_translator"),
                        IsCancel = true,
                        StyleKey = "SimpleButton"
                    }
                }
            });

            if (MainWindow != null && MainWindow.IsVisible)
                dialog.Owner = MainWindow;

            dialog.ShowDialog();
        }


        private void ShowAlreadyRunningDialog()
        {
            LoadLanguageDictionaryEarly();

            var dialog = new GenericDialogWindow(new GenericDialogOptions
            {
                Title = TranslationHelper.Translate("AppAlreadyRunningTitle_translator"),
                Message = TranslationHelper.Translate("AppAlreadyRunning_translator"),
                Topmost = true,
                IsModal = true,
                Buttons =
                {
                    new DialogButtonConfig
                    {
                        Text = TranslationHelper.Translate("Close_translator"),
                        IsDefault = true,
                        IsCancel = true,
                    }
                }
            });
            dialog.ShowDialog();
        }

        // LanguageService.LoadLanguage() normally runs later, in OnInitialized — but the
        // second-instance path exits before that, so merge the language dictionary here
        // so TranslationHelper.Translate() can resolve keys for this dialog.
        private static void LoadLanguageDictionaryEarly()
        {
            var language = "en";
            try
            {
                var settingsPath = @"C:\ProgramData\RdpScopeToggler\Settings.json";
                if (File.Exists(settingsPath))
                {
                    using var doc = JsonDocument.Parse(File.ReadAllText(settingsPath));
                    if (doc.RootElement.TryGetProperty("Language", out var langProp))
                    {
                        var value = langProp.GetString();
                        if (!string.IsNullOrWhiteSpace(value))
                            language = value;
                    }
                }
            }
            catch { }

            try
            {
                Current.Resources.MergedDictionaries.Add(new ResourceDictionary
                {
                    Source = new Uri($"Resources/Language/StringResources.{language}.xaml", UriKind.Relative)
                });
            }
            catch { }
        }

        private void ShowMainWindow()
        {
            if (MainWindow == null)
                return;

            MainWindow.Show();
            if (MainWindow.WindowState == WindowState.Minimized)
                MainWindow.WindowState = WindowState.Normal;
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
