using System.Runtime.InteropServices;
using System;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Threading;

namespace RdpScopeToggler.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        [DllImport("dwmapi.dll")]
        private static extern int DwmExtendFrameIntoClientArea(IntPtr hwnd, ref MARGINS margins);

        [StructLayout(LayoutKind.Sequential)]
        private struct MARGINS
        {
            public int cxLeftWidth;
            public int cxRightWidth;
            public int cyTopHeight;
            public int cyBottomHeight;
        }

        public MainWindow()
        {
            InitializeComponent();
            this.Closing += MainWindow_Closing;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            var windowHelper = new WindowInteropHelper(this);

            var margins = new MARGINS
            {
                cxLeftWidth = 1,
                cxRightWidth = 1,
                cyTopHeight = 1,
                cyBottomHeight = 1
            };

            DwmExtendFrameIntoClientArea(windowHelper.Handle, ref margins);


            // יישר את הגודל לפי התוכן
            SizeToContent = SizeToContent.WidthAndHeight;

            // ברגע ש-WPF תסיים את המדידה והפריסה, נבצע החזרה למצב רגיל
            Dispatcher.BeginInvoke(new Action(() =>
            {
                SizeToContent = SizeToContent.Manual;
            }), DispatcherPriority.Loaded);
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // Cancel the close
            e.Cancel = true;

            // Hide the window instead of closing
            this.Hide();
        }
    }
}