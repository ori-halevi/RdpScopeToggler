using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace RdpScopeToggler.Views
{
    /// <summary>
    /// Interaction logic for TitleBarUserControl
    /// </summary>
    public partial class TitleBarUserControl : UserControl
    {
        public TitleBarUserControl()
        {
            InitializeComponent();
        }

        private Window GetWindow() => Window.GetWindow(this);

        private void Minimize_Click(object sender, RoutedEventArgs e)
        {
            GetWindow().WindowState = WindowState.Minimized;
        }

        /*private void MaximizeRestore_Click(object sender, RoutedEventArgs e)
        {
            var window = GetWindow();
            if (window.WindowState == WindowState.Maximized)
            {
                window.WindowState = WindowState.Normal;
                MaximizeIcon.Visibility = Visibility.Visible;
                RestoreIcon.Visibility = Visibility.Collapsed;
            }
            else
            {
                window.WindowState = WindowState.Maximized;
                MaximizeIcon.Visibility = Visibility.Collapsed;
                RestoreIcon.Visibility = Visibility.Visible;
            }
        }*/

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            GetWindow().Close();
        }

        private void CloseButton_MouseEnter(object sender, MouseEventArgs e)
        {
            (sender as Button).Background = System.Windows.Media.Brushes.Red;
        }

        private void CloseButton_MouseLeave(object sender, MouseEventArgs e)
        {
            (sender as Button).Background = System.Windows.Media.Brushes.Transparent;
        }

        // גרירת החלון בלחיצה על אזור הכותרת
        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            var window = GetWindow();
            if (window != null && e.ClickCount == 1)
                window.DragMove();
        }
    }
}