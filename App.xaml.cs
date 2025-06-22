using System.Windows;
using Prism.Ioc;
using RdpScopeToggler.Views;
using System.Drawing;
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
        public static System.Windows.Forms.NotifyIcon notifyIcon;
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
