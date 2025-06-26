using System.Windows;
using Prism.Ioc;
using System.Drawing;
using Prism.Navigation.Regions;
using RdpScopeToggler.Views;
using RdpScopeToggler.Stores;
using RdpScopeToggler.Services.FilesService;
using RdpScopeToggler.Services.RdpService;
using System.Threading.Tasks;
using System.Windows.Threading;
using System;
using RdpScopeToggler.Services.NotificationService;
using RdpScopeToggler.Models;
using System.Collections.Generic;

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

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            string pathToToastMessageFile = "C:\\ProgramData\\RdpScopeToggler\\ToastMessage.txt";
            string pathToToastSoftwareFile = "C:\\ProgramData\\RdpScopeToggler\\RdpScopeTogglerToastListener\\RdpScopeTogglerToastListener.exe";
            string sourceBaseDirectory = "Assets\\Deployment\\RdpScopeTogglerToastListener";

            containerRegistry.RegisterSingleton<INotificationService>(() =>
            {
                var notificationService = new NotificationService(pathToToastMessageFile, pathToToastSoftwareFile, sourceBaseDirectory);
                notificationService.NotificationToolInstalled += App_NotificationToolInstalled;
                return notificationService;
            });
            Container.Resolve<INotificationService>().InitializeInstallation();

            containerRegistry.RegisterSingleton<IRdpService, RdpService>();
            containerRegistry.RegisterSingleton<IFilesService, FilesService>();
            containerRegistry.RegisterSingleton<TaskInfoStore>();

            containerRegistry.RegisterForNavigation<HomeUserControl>();
            containerRegistry.RegisterForNavigation<WaitingUserControl>();
            containerRegistry.RegisterForNavigation<TaskUserControl>();
            containerRegistry.RegisterForNavigation<SettingsUserControl>();
            containerRegistry.RegisterForNavigation<WhiteListUserControl>();
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

            Application.Current.ShutdownMode = ShutdownMode.OnExplicitShutdown;

            base.OnStartup(e);

            notifyIcon = new System.Windows.Forms.NotifyIcon();
            notifyIcon.Icon = new Icon("Assets/remote-desktop.ico");
            notifyIcon.Visible = true;
            notifyIcon.Text = "Rdp Scope Toggler";

            var contextMenu = new System.Windows.Forms.ContextMenuStrip();

            contextMenu.Items.Add("פתח חלון", null, (s, ea) => ShowMainWindow());
            contextMenu.Items.Add("יציאה", null, (s, ea) =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    var confirmWindow = new ExitConfirmationWindow();
                    confirmWindow.ShowDialog();

                    if (confirmWindow.UserConfirmed)
                    {
                        App.notifyIcon.Visible = false;
                        Application.Current.Shutdown();
                    }
                });
            });

            notifyIcon.ContextMenuStrip = contextMenu;

            notifyIcon.MouseClick += (s, ea) =>
            {
                if (ea.Button == System.Windows.Forms.MouseButtons.Left)
                    ShowMainWindow();
            };
        }


        private void App_NotificationToolInstalled()
        {
            var options = new GenericDialogOptions
            {
                Title = "הפעלה מחדש מומלצת",
                Message = "המערכת זיהתה הפעלה ראשונית של התוכנה,\nאם לא תפעיל מחדש את המערכת יתכן שמשתמשים אחרים לא יקבלו התראות.",
                OnClose = () => { },
                IsModal = true,
                Topmost = true,
            };

            var dialog = new GenericDialogWindow(options);
            if (options.IsModal)
                dialog.ShowDialog();
            else
                dialog.Show();
        }

        private void ShowMainWindow()
        {
            if (MainWindow == null)
                return;

            MainWindow.Show();
            MainWindow.Activate();
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
