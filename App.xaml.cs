using Microsoft.Extensions.DependencyInjection;
using Prism.Container.Unity;
using Prism.Ioc;
using Prism.Navigation.Regions;
using RdpScopeCommands;
using RdpScopeToggler.Managers;
using RdpScopeToggler.Services.FilesService;
using RdpScopeToggler.Services.LanguageService;
using RdpScopeToggler.Services.LoggerService;
using RdpScopeToggler.Services.PipeClientService;
using RdpScopeToggler.Services.UpdateCheckerService;
using RdpScopeToggler.ViewModels;
using RdpScopeToggler.Views;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
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
            containerRegistry.RegisterSingleton<IUpdateCheckerService, UpdateCheckerService>();
            // Unity + IHttpClientFactory
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddHttpClient();
            var serviceProvider = serviceCollection.BuildServiceProvider();

            containerRegistry.RegisterInstance(serviceProvider.GetRequiredService<IHttpClientFactory>());

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

            // Copy files of RdpScopeTogglerService
            string sourcePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Deployment");
            string targetPath = @"C:\ProgramData\RdpScopeToggler";

            string sourceDelServiceFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Deployment", "RdpScopeService");
            string targetDelServicePath = @"C:\ProgramData\RdpScopeToggler\RdpScopeService";
            CopySingleFile(sourceDelServiceFilePath, targetDelServicePath, "DelService.bat");

            string DelServiceFilePath = @"C:\ProgramData\RdpScopeToggler\RdpScopeService\DelService.bat";
            var processStartInfo1 = new ProcessStartInfo
            {
                FileName = DelServiceFilePath,
                UseShellExecute = true, // חשוב כדי שהמערכת תדע להריץ BAT
                CreateNoWindow = false, // אם אתה רוצה לראות חלון CMD, תשאיר true כדי להסתיר
                Verb = "runas" // אם נדרש להריץ כאדמין
            };
            if (File.Exists(DelServiceFilePath)) // TODO: Change the algorithm so it will only run after the file exists (asynchronous something)
                Process.Start(processStartInfo1);
            else
                MessageBox.Show("Batch file not found!");

            CopyDirectory(sourcePath, targetPath);
            string batchFilePath = @"C:\ProgramData\RdpScopeToggler\RdpScopeService\RunService.bat";

            var processStartInfo = new ProcessStartInfo
            {
                FileName = batchFilePath,
                UseShellExecute = true, // חשוב כדי שהמערכת תדע להריץ BAT
                CreateNoWindow = false, // אם אתה רוצה לראות חלון CMD, תשאיר true כדי להסתיר
                Verb = "runas" // אם נדרש להריץ כאדמין
            };
            if (File.Exists(batchFilePath)) // TODO: Change the algorithm so it will only run after the file exists (asynchronous something)
                Process.Start(processStartInfo);
            else
                MessageBox.Show("Batch file not found!");

            await Task.Delay(1000);
            //
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
                Debug.WriteLine("Couldn't connect to the server.");
            }

        }

        /// <summary>
        /// Copy folder recursively while keeping directory structure.
        /// </summary>
        private void CopyDirectory(string sourceDir, string destinationDir)
        {
            Directory.CreateDirectory(destinationDir);

            foreach (string filePath in Directory.GetFiles(sourceDir))
            {
                string fileName = Path.GetFileName(filePath);
                string destFilePath = Path.Combine(destinationDir, fileName);

                try
                {
                    File.Copy(filePath, destFilePath, overwrite: true);
                }
                catch (IOException)
                {
                    // הקובץ כנראה נעול או בשימוש, מדלגים עליו
                    // אפשר גם לכתוב ללוג אם רוצים
                }
            }

            foreach (string dirPath in Directory.GetDirectories(sourceDir))
            {
                string dirName = Path.GetFileName(dirPath);
                string destDirPath = Path.Combine(destinationDir, dirName);
                CopyDirectory(dirPath, destDirPath);
            }
        }

        /// <summary>
        /// Copy a single file from source to destination folder.
        /// </summary>
        private void CopySingleFile(string sourceDir, string destinationDir, string fileName)
        {
            string sourceFile = Path.Combine(sourceDir, fileName);
            string destFile = Path.Combine(destinationDir, fileName);

            if (!File.Exists(sourceFile))
                return; // הקובץ לא קיים במקור – פשוט לא עושים כלום

            Directory.CreateDirectory(destinationDir);

            try
            {
                File.Copy(sourceFile, destFile, overwrite: true);
            }
            catch (IOException)
            {
                // אם הקובץ נעול או בשימוש – אפשר לדלג או לטפל אחרת
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
